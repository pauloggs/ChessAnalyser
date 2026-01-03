using Interfaces.DTO;
using Services.Helpers.BoardUpdater;

namespace Services.Helpers
{
    public interface IBoardPositionCalculator
    {
        BoardPosition GetBoardPositionFromPly(
                BoardPosition previousBoardPosition,
                Ply ply);

        void UpdateCurrentBoardPositionWithMove(
            BoardPosition currentBoardPosition,
            Ply ply,
            int sourceSquare,
            int destinationSquare);
    }

    public class BoardPositionCalculator (
        IBoardPositionCalculatorHelper boardPositionCalculatorHelper,
        IUpdaterEnPassant updaterEnPassant,
        IUpdaterKingsideCastling updaterKingsideCastling,
        IUpdaterNonPromotion updaterNonPromotion,
        IUpdaterPromotion updaterPromotion,
        IUpdaterQueensideCastling updaterQueensideCastling) : IBoardPositionCalculator
    {
        public BoardPosition GetBoardPositionFromPly(
                BoardPosition previousBoardPosition,
                Ply ply)
        {
            string piecePositionsKey = ply.Colour.ToString() + ply.Piece.Name;

            if (ply.IsEnpassant)
            {
                // Handle en passant move, returning updated board position
                return boardPositionCalculatorHelper.GetBoardPositionFromEnPassant(
                    previousBoardPosition,
                    ply);
            }
            else if (ply.IsPieceMove)
            {
                // handle a piece move (including pawn moves), returning updated board position
                if (ply.IsPromotion)
                {
                    // Handle promotion move, returning updated board position
                    return boardPositionCalculatorHelper.GetBoardPositionFromPromotion(
                        previousBoardPosition,
                        ply);
                }
                else
                {
                    // Handle non-promotion piece move, returning updated board position
                    return boardPositionCalculatorHelper.GetBoardPositionFromNonPromotion(
                        previousBoardPosition,
                        ply);
                }
            }
            else if (ply.IsKingsideCastling)
            {
                // Handle kingside castling move, returning updated board position
                return boardPositionCalculatorHelper.GetBoardPositionFromKingSideCastling(
                    previousBoardPosition, 
                    ply);
            }
            else if (ply.IsQueensideCastling)
            {
                // Handle queenside castling move, returning updated board position
                return boardPositionCalculatorHelper.GetBoardPositionFromQueenSideCastling(
                    previousBoardPosition,
                    ply);
            }
            else
            {
                throw new Exception("Unrecognized move type");
            }
            
            return new BoardPosition();
        }

        public void UpdateCurrentBoardPositionWithMove(
            BoardPosition currentBoardPosition,
            Ply ply,
            int sourceSquare,
            int destinationSquare)
        {
            //string piecePositionsKey = new([colour, ply.Piece]);
            string piecePositionsKey = ply.Colour.ToString() + ply.Piece.Name;

            // if it's a capture, then remove the piece from the opposite colour bitboard
            // need to find the piece! or just run through them all

            if (ply.IsEnpassant)
            {
                updaterEnPassant.UpdateBoard(
                        currentBoardPosition,
                        piecePositionsKey,
                        ply,
                        sourceSquare,
                        destinationSquare);
            }
            else if (ply.IsPieceMove)
            {
                if (ply.IsPromotion)
                {
                    updaterPromotion.UpdateBoard(
                        currentBoardPosition,
                        piecePositionsKey,
                        ply,
                        sourceSquare,
                        destinationSquare);
                }
                else
                {
                    updaterNonPromotion.UpdateBoard(
                        currentBoardPosition,
                        piecePositionsKey,
                        ply,
                        sourceSquare,
                        destinationSquare);
                }                    
            }
            else if (ply.IsKingsideCastling)
            {
                updaterKingsideCastling.UpdateBoard(
                        currentBoardPosition,
                        piecePositionsKey,
                        ply,
                        sourceSquare,
                        destinationSquare);
            }
            else if (ply.IsQueensideCastling)
            {
                updaterQueensideCastling.UpdateBoard(
                        currentBoardPosition,
                        piecePositionsKey,
                        ply,
                        sourceSquare,
                        destinationSquare);
            }
            //else if (ply.IsEnpassant)
            //{
            //    // move the pawn
            //    var pawnPositions = currentBoardPosition.PiecePositions[piecePositionsKey];
            //    currentBoardPosition.PiecePositions[piecePositionsKey]
            //        = bitBoardManipulator.MovePiece(pawnPositions, sourceSquare, destinationSquare);

            //    // remove the captured pawn
            //    var oppCol = ply.Colour == Colour.W ? 'B' : 'W';
            //    string oppPawnPositionsKey = new([oppCol, 'P']);
            //    currentBoardPosition.PiecePositions[oppPawnPositionsKey]
            //        = bitBoardManipulator.RemovePiece(
            //            currentBoardPosition.PiecePositions[oppPawnPositionsKey],
            //            destinationSquare + (ply.Colour == Colour.W ? -8 : 8));
            //}
            else
            {
                throw new Exception($"Move is invalid {ply.RawMove}");
            }
        }
    }
}
