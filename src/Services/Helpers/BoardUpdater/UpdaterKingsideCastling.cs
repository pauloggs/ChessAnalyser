using Interfaces.DTO;
using static Interfaces.Constants;

namespace Services.Helpers.BoardUpdater
{
    public interface IUpdaterKingsideCastling : IBoardUpdater
    {
    }

    public class UpdaterKingsideCastling(IBitBoardManipulator bitBoardManipulator) : IUpdaterKingsideCastling
    {
        public void UpdateBoard(
            BoardPosition currentBoardPosition,
            string piecePositionsKey,
            Ply ply,
            int sourceSquare,
            int destinationSquare)
        {
            // handle king-side castling for that particular colour
            if (ply.Colour == Colour.W)
            {
                // move the king
                var kingPositions = currentBoardPosition.PiecePositions["WK"];

                currentBoardPosition.PiecePositions["WK"]
                    = bitBoardManipulator.PiecePositionsAfterMove(kingPositions, 4, 6);

                // then move the rook
                var rookPositions = currentBoardPosition.PiecePositions["WR"];

                currentBoardPosition.PiecePositions["WR"]
                    = bitBoardManipulator.PiecePositionsAfterMove(rookPositions, 7, 5);
            }
            else
            {
                // move the king
                var kingPositions = currentBoardPosition.PiecePositions["BK"];

                currentBoardPosition.PiecePositions["BK"]
                    = bitBoardManipulator.PiecePositionsAfterMove(kingPositions, 60, 62);

                // then move the rook
                var rookPositions = currentBoardPosition.PiecePositions["BR"];

                currentBoardPosition.PiecePositions["BR"]
                    = bitBoardManipulator.PiecePositionsAfterMove(rookPositions, 63, 61);
            }
        }
    }
}
