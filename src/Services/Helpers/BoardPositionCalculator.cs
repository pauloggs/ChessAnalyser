using Interfaces.DTO;

namespace Services.Helpers
{
    public interface IBoardPositionCalculator
    {
        BoardPosition GetBoardPositionFromPly(
                BoardPosition previousBoardPosition,
                Ply ply);
    }

    public class BoardPositionCalculator(
        IBoardPositionCalculatorHelper boardPositionCalculatorHelper) : IBoardPositionCalculator
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
        }
    }
}