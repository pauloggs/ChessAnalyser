using Interfaces;
using Interfaces.DTO;
using static Interfaces.Constants;

namespace Services.Helpers
{
    public interface IBoardPositionCalculatorHelper
    {
        BoardPosition GetBoardPositionFromEnPassant(
            BoardPosition previousBoardPosition,
            Ply ply);

        BoardPosition GetBoardPositionFromPromotion(
            BoardPosition previousBoardPosition,
            Ply ply);

        /// <summary>
        /// Returns a new BoardPosition after applying a non-promotion move described by the given Ply.
        /// This is done by moving the piece from the source square to the destination square for the specified piece and colour 
        /// in the BoardPosition's PiecePositions dictionary.
        /// </summary>
        /// <param name="previousBoardPosition"></param>
        /// <param name="ply"></param>
        /// <returns></returns>
        BoardPosition GetBoardPositionFromNonPromotion(
            BoardPosition previousBoardPosition,
            Ply ply);

        BoardPosition GetBoardPositionFromKingSideCastling(
            BoardPosition previousBoardPosition,
            Ply ply);
        BoardPosition GetBoardPositionFromQueenSideCastling(
            BoardPosition previousBoardPosition,
            Ply ply);   
    }

    public class BoardPositionCalculatorHelper(IBitBoardManipulator bitBoardManipulator) : IBoardPositionCalculatorHelper
    {
        public BoardPosition GetBoardPositionFromEnPassant(BoardPosition previousBoardPosition, Ply ply)
        {
            // 1. Start with a deep copy
            BoardPosition newBoardPosition = previousBoardPosition.DeepCopy();

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
                throw new InvalidOperationException("En-passant capture failed: No enemy pawn at the expected capture square.");
            }

            // 4. Remove the captured enemy pawn
            bitBoardManipulator.RemovePiece(
                newBoardPosition,
                capturedPawnSquare,
                ply.OppositeColour);

            // 5. Move the friendly pawn from source to destination
            // This removes from source and adds to destination
            bitBoardManipulator.MovePiece(
                newBoardPosition,
                ply);

            return newBoardPosition;
        }

        public BoardPosition GetBoardPositionFromPromotion(BoardPosition previousBoardPosition, Ply ply)
        {
            // 1. Start with a copy of the board
            BoardPosition newBoardPosition = previousBoardPosition.DeepCopy();

            // 2. Handle the Capture (if applicable)
            if (ply.IsCapture)
            {
                var (targetPiece, targetColour) = bitBoardManipulator.ReadSquare(
                    newBoardPosition,
                    ply.DestinationSquare);

                if (targetPiece == null || targetColour == ply.Colour)
                {
                    throw new InvalidOperationException($"Invalid capture at {ply.DestinationSquare} during promotion.");
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
                    throw new InvalidOperationException($"Square {ply.DestinationSquare} is occupied; cannot promote.");
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

        public BoardPosition GetBoardPositionFromNonPromotion(BoardPosition previousBoardPosition, Ply ply)
        {
            BoardPosition newBoardPosition
                = previousBoardPosition.DeepCopy();

            var newPositionsForPlyPiece = newBoardPosition.PiecePositions[ply.PiecePositionsKey];

            if (ply.IsCapture)
            {
                // Ensure that there is an opposing piece at the destination square
                var (occupyingPiece, occupyingColour) = bitBoardManipulator.ReadSquare(
                    newBoardPosition,
                    ply.DestinationSquare);

                // Throw an exception if there is no opposing piece at the destination square
                // or if the occupying piece is of the same colour as the moving piece
                if (occupyingPiece == null || occupyingColour == ply.Colour)
                {
                    throw new InvalidOperationException($"There is no opposing piece at the destination square {ply.DestinationSquare} for a capture move.");
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
                    throw new InvalidOperationException($"There is a piece at the destination square {ply.DestinationSquare} for a non-capture move.");
                }
            }

            // Move the piece from source to destination
            bitBoardManipulator.MovePiece(
                newBoardPosition,
                ply);

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
