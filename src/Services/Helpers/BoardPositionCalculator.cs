using Interfaces.DTO;

namespace Services.Helpers
{
    public interface IBoardPositionCalculator
    {
        BoardPosition GetBoardPositionFromPly(
                BoardPosition previousBoardPosition,
                Ply ply,
                string? parsingContext = null);
    }

    public class BoardPositionCalculator(
        IBoardPositionCalculatorHelper boardPositionCalculatorHelper) : IBoardPositionCalculator
    {
        public BoardPosition GetBoardPositionFromPly(
                BoardPosition previousBoardPosition,
                Ply ply,
                string? parsingContext = null)
        {
            string piecePositionsKey = ply.Colour.ToString() + ply.Piece.Name;

            if (ply.IsEnpassant)
            {
                return boardPositionCalculatorHelper.GetBoardPositionFromEnPassant(
                    previousBoardPosition,
                    ply,
                    parsingContext);
            }
            else if (ply.IsPieceMove)
            {
                if (ply.IsPromotion)
                {
                    return boardPositionCalculatorHelper.GetBoardPositionFromPromotion(
                        previousBoardPosition,
                        ply,
                        parsingContext);
                }
                else
                {
                    return boardPositionCalculatorHelper.GetBoardPositionFromNonPromotion(
                        previousBoardPosition,
                        ply,
                        parsingContext);
                }
            }
            else if (ply.IsKingsideCastling)
            {
                return boardPositionCalculatorHelper.GetBoardPositionFromKingSideCastling(
                    previousBoardPosition,
                    ply);
            }
            else if (ply.IsQueensideCastling)
            {
                return boardPositionCalculatorHelper.GetBoardPositionFromQueenSideCastling(
                    previousBoardPosition,
                    ply);
            }
            else
            {
                throw new Exception(string.IsNullOrEmpty(parsingContext) ? "Unrecognized move type" : $"Unrecognized move type ({parsingContext})");
            }
        }
    }
}