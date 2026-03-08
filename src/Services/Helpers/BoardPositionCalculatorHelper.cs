using Interfaces;
using Interfaces.DTO;
using static Interfaces.Constants;

namespace Services.Helpers
{
    public interface IBoardPositionCalculatorHelper
    {
        BoardPosition GetBoardPositionFromEnPassant(
            BoardPosition previousBoardPosition,
            Ply ply,
            string? parsingContext = null);

        BoardPosition GetBoardPositionFromPromotion(
            BoardPosition previousBoardPosition,
            Ply ply,
            string? parsingContext = null);

        BoardPosition GetBoardPositionFromNonPromotion(
            BoardPosition previousBoardPosition,
            Ply ply,
            string? parsingContext = null);

        BoardPosition GetBoardPositionFromKingSideCastling(
            BoardPosition previousBoardPosition,
            Ply ply);
        BoardPosition GetBoardPositionFromQueenSideCastling(
            BoardPosition previousBoardPosition,
            Ply ply);
    }

    public class BoardPositionCalculatorHelper(IBitBoardManipulator bitBoardManipulator) : IBoardPositionCalculatorHelper
    {
        private static string AppendContext(string message, string? ctx) =>
            string.IsNullOrEmpty(ctx) ? message : $"{message} ({ctx})";

        private static string GetPieceSquaresDiagnostic(BoardPosition board, Ply ply)
        {
            if (!board.PiecePositions.TryGetValue(ply.PiecePositionsKey, out ulong bb))
                return $"[{ply.PiecePositionsKey}] bitboard missing.";
            var squares = new List<string>();
            for (int sq = 0; sq < 64; sq++)
            {
                if ((bb & (1UL << sq)) != 0)
                    squares.Add(sq.Algebraic());
            }
            return squares.Count == 0
                ? $"[{ply.PiecePositionsKey}] has no pieces on board."
                : $"[{ply.PiecePositionsKey}] on: {string.Join(", ", squares)}.";
        }

        public BoardPosition GetBoardPositionFromEnPassant(BoardPosition previousBoardPosition, Ply ply, string? parsingContext = null)
        {
            // 1. Start with a deep copy
            BoardPosition newBoardPosition = previousBoardPosition.DeepCopy();

            // En-passant is only legal immediately after a double pawn push.
            if (!previousBoardPosition.EnPassantTargetFile.HasValue)
            {
                throw new InvalidOperationException(AppendContext("En-passant capture failed: no en-passant target is available.", parsingContext));
            }

            var destinationFileChar = FileIds[ply.DestinationFile];

            if (char.ToUpperInvariant(previousBoardPosition.EnPassantTargetFile.Value) != destinationFileChar)
            {
                throw new InvalidOperationException(AppendContext("En-passant capture failed: destination file does not match en-passant target file.", parsingContext));
            }

            // 2. Identify the square of the pawn being captured
            // If White is moving to Rank 6, the captured Black pawn is on Rank 5
            // If Black is moving to Rank 3, the captured White pawn is on Rank 4
            int captureRank = (ply.Colour == Colour.W) ? 4 : 3; // 0-indexed (Rank 5 or 4)
            int capturedPawnSquare = captureRank * 8 + ply.DestinationFile;

            // 3. Validation: Ensure the square behind the destination contains an enemy pawn
            var (occupyingPiece, occupyingColour) = bitBoardManipulator.ReadSquare(
                newBoardPosition,
                capturedPawnSquare);

            if (occupyingPiece != Constants.Pieces['P'] || occupyingColour == ply.Colour)
            {
                throw new InvalidOperationException(AppendContext("En-passant capture failed: no enemy pawn at the expected capture square.", parsingContext));
            }

            // 4. Remove the captured enemy pawn
            bitBoardManipulator.RemovePiece(
                newBoardPosition,
                capturedPawnSquare,
                ply.OppositeColour);

            // 5. Move the friendly pawn from source to destination
            // This removes from source and adds to destination
            try
            {
                bitBoardManipulator.MovePiece(
                    newBoardPosition,
                    ply);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new ArgumentOutOfRangeException(ex.ParamName, AppendContext(ex.Message, parsingContext));
            }

            // 6. Clear en-passant target for the resulting position
            newBoardPosition.EnPassantTargetFile = null;

            return newBoardPosition;
        }

        public BoardPosition GetBoardPositionFromPromotion(BoardPosition previousBoardPosition, Ply ply, string? parsingContext = null)
        {
            // 1. Start with a copy of the board
            BoardPosition newBoardPosition = previousBoardPosition.DeepCopy();

            // Promotion always clears any en-passant target
            newBoardPosition.EnPassantTargetFile = null;

            // 2. Handle the Capture (if applicable)
            if (ply.IsCapture)
            {
                var (targetPiece, targetColour) = bitBoardManipulator.ReadSquare(
                    newBoardPosition,
                    ply.DestinationSquare);

                if (targetPiece == null || targetColour == ply.Colour)
                {
                    throw new InvalidOperationException(AppendContext($"Invalid capture at {ply.DestinationSquare} during promotion.", parsingContext));
                }

                // Remove the captured piece
                bitBoardManipulator.RemovePiece(
                    newBoardPosition,
                    ply.DestinationSquare,
                    ply.OppositeColour);
            }
            else
            {
                // For non-captures, ensure the square is empty
                var (targetPiece, _) = bitBoardManipulator.ReadSquare(newBoardPosition, ply.DestinationSquare);
                if (targetPiece != null)
                {
                    throw new InvalidOperationException(AppendContext($"Square {ply.DestinationSquare} is occupied; cannot promote.", parsingContext));
                }
            }

            // 3. Remove the Pawn from the Source
            // Note: Assuming 'ply.Piece' here is 'P' (the pawn that is moving)
            bitBoardManipulator.RemovePiece(
                newBoardPosition,
                ply.SourceSquare,
                ply.Colour);

            // 4. Add the Promoted Piece to the Destination
            // In your Ply object, 'ply.PromotedPiece' should store 'Q', 'R', 'B', or 'N'
            var promotedPieceKey = ply.Colour.ToString() + ply.PromotionPiece!.Name;


            newBoardPosition.PiecePositions[promotedPieceKey] = bitBoardManipulator.AddPiece(
                newBoardPosition.PiecePositions[promotedPieceKey],
                ply.DestinationSquare);

            return newBoardPosition;
        }

        public BoardPosition GetBoardPositionFromNonPromotion(BoardPosition previousBoardPosition, Ply ply, string? parsingContext = null)
        {
            BoardPosition newBoardPosition
                = previousBoardPosition.DeepCopy();

            // By default, en-passant is only valid for a single reply move.
            // Unless we detect a new double pawn push below, clear the target.
            newBoardPosition.EnPassantTargetFile = null;

            var newPositionsForPlyPiece = newBoardPosition.PiecePositions[ply.PiecePositionsKey];

            if (ply.IsCapture)
            {
                // Ensure that there is an opposing piece at the destination square.
                // If the destination is empty for a pawn capture, this may be an en-passant move.
                var (occupyingPiece, occupyingColour) = bitBoardManipulator.ReadSquare(
                    previousBoardPosition,
                    ply.DestinationSquare);

                if (occupyingPiece == null)
                {
                    // Potential en-passant: only valid for pawn captures.
                    if (ply.IsPawnMove)
                    {
                        ply.IsEnpassant = true;
                        return GetBoardPositionFromEnPassant(previousBoardPosition, ply, parsingContext);
                    }

                    throw new InvalidOperationException(AppendContext($"There is no opposing piece at the destination square {ply.DestinationSquare} for a capture move.", parsingContext));
                }

                if (occupyingColour == ply.Colour)
                {
                    throw new InvalidOperationException(AppendContext($"There is no opposing piece at the destination square {ply.DestinationSquare} for a capture move.", parsingContext));
                }

                // Remove the opposing piece from the destination square
                bitBoardManipulator.RemovePiece(
                    newBoardPosition,
                    ply.DestinationSquare,
                    ply.OppositeColour);
            }
            else
            {
                // Ensure that there is no opposing piece at the destination square
                var (occupyingPiece, _) = bitBoardManipulator.ReadSquare(
                    newBoardPosition,
                    ply.DestinationSquare);

                if (occupyingPiece != null)
                {
                    throw new InvalidOperationException(AppendContext($"There is a piece at the destination square {ply.DestinationSquare} for a non-capture move.", parsingContext));
                }
            }

            // Move the piece from source to destination
            try
            {
                bitBoardManipulator.MovePiece(
                    newBoardPosition,
                    ply);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                var diag = GetPieceSquaresDiagnostic(previousBoardPosition, ply);
                var msg = string.IsNullOrEmpty(diag) ? ex.Message : $"{ex.Message} {diag}";
                throw new ArgumentOutOfRangeException(ex.ParamName, AppendContext(msg, parsingContext));
            }

            // If this is a quiet double pawn push, set the en-passant target file.
            // Derive file from DestinationSquare so we are not dependent on ply.DestinationFile being set by the caller.
            if (ply.IsPawnMove && !ply.IsCapture && !ply.IsPromotion)
            {
                var sourceRank = ply.SourceSquare / 8;
                var destinationRank = ply.DestinationSquare / 8;

                if (Math.Abs(destinationRank - sourceRank) == 2)
                {
                    var destinationFile = ply.DestinationSquare % 8;
                    newBoardPosition.EnPassantTargetFile = FileIds[destinationFile];
                }
            }

            return newBoardPosition;
        }        

        public BoardPosition GetBoardPositionFromKingSideCastling(BoardPosition previousBoardPosition, Ply ply)
        {
            // White Castling
            const int WhiteKingSource = Squares.E1;
            const int WhiteKingDestination = Squares.G1;
            const int WhiteRookSource = Squares.H1;
            const int WhiteRookDestination = Squares.F1;

            // Black Castling
            const int BlackKingSource = Squares.E8;
            const int BlackKingDestination = Squares.G8;
            const int BlackRookSource = Squares.H8;
            const int BlackRookDestination = Squares.F8;

            BoardPosition currentBoardPosition
                = previousBoardPosition.DeepCopy();

            // Castling clears any en-passant target
            currentBoardPosition.EnPassantTargetFile = null;

            // handle king-side castling for that particular colour
            if (ply.Colour == Colour.W)
            {
                // move the king
                var kingPositions = currentBoardPosition.PiecePositions["WK"];

                currentBoardPosition.PiecePositions["WK"]
                    = bitBoardManipulator.MovePiece(
                        kingPositions,
                        WhiteKingSource,
                        WhiteKingDestination);

                // then move the rook
                var rookPositions = currentBoardPosition.PiecePositions["WR"];

                currentBoardPosition.PiecePositions["WR"]
                    = bitBoardManipulator.MovePiece(
                        rookPositions,
                        WhiteRookSource,
                        WhiteRookDestination);
            }
            else
            {
                // move the king
                var kingPositions = currentBoardPosition.PiecePositions["BK"];

                currentBoardPosition.PiecePositions["BK"]
                    = bitBoardManipulator.MovePiece(
                        kingPositions,
                        BlackKingSource,
                        BlackKingDestination);

                // then move the rook
                var rookPositions = currentBoardPosition.PiecePositions["BR"];

                currentBoardPosition.PiecePositions["BR"]
                    = bitBoardManipulator.MovePiece(
                        rookPositions,
                        BlackRookSource,
                        BlackRookDestination);
            }

            return currentBoardPosition;
        }

        public BoardPosition GetBoardPositionFromQueenSideCastling(BoardPosition previousBoardPosition, Ply ply)
        {
            // White Castling
            const int WhiteKingSource = Squares.E1;
            const int WhiteKingDestination = Squares.C1;
            const int WhiteRookSource = Squares.A1;
            const int WhiteRookDestination = Squares.D1;

            // Black Castling
            const int BlackKingSource = Squares.E8;
            const int BlackKingDestination = Squares.C8;
            const int BlackRookSource = Squares.A8;
            const int BlackRookDestination = Squares.D8;

            BoardPosition currentBoardPosition
                = previousBoardPosition.DeepCopy();

            // Castling clears any en-passant target
            currentBoardPosition.EnPassantTargetFile = null;

            // handle king-side castling for that particular colour
            if (ply.Colour == Colour.W)
            {
                // move the king
                var kingPositions = currentBoardPosition.PiecePositions["WK"];

                currentBoardPosition.PiecePositions["WK"]
                    = bitBoardManipulator.MovePiece(
                        kingPositions,
                        WhiteKingSource,
                        WhiteKingDestination);

                // then move the rook
                var rookPositions = currentBoardPosition.PiecePositions["WR"];

                currentBoardPosition.PiecePositions["WR"]
                    = bitBoardManipulator.MovePiece(
                        rookPositions,
                        WhiteRookSource,
                        WhiteRookDestination);
            }
            else
            {
                // move the king
                var kingPositions = currentBoardPosition.PiecePositions["BK"];

                currentBoardPosition.PiecePositions["BK"]
                    = bitBoardManipulator.MovePiece(
                        kingPositions,
                        BlackKingSource,
                        BlackKingDestination);

                // then move the rook
                var rookPositions = currentBoardPosition.PiecePositions["BR"];

                currentBoardPosition.PiecePositions["BR"]
                    = bitBoardManipulator.MovePiece(
                        rookPositions,
                        BlackRookSource,
                        BlackRookDestination);
            }

            return currentBoardPosition;
        }
    }
}
