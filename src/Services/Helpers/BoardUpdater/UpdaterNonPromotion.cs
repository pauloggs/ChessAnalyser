using Interfaces;
using Interfaces.DTO;
using static Interfaces.Constants;

namespace Services.Helpers.BoardUpdater
{
    public interface IUpdaterNonPromotion : IBoardUpdater
    {
    }

    public class UpdaterNonPromotion(IBitBoardManipulator bitBoardManipulator) : IUpdaterNonPromotion
    {
        public void UpdateBoard(
            BoardPosition currentBoardPosition, 
            string piecePositionsKey, 
            Ply ply,
            int sourceSquare, 
            int destinationSquare)
        {
            var piecePositions = currentBoardPosition.PiecePositions[piecePositionsKey];

            var newPiecePositions
                = bitBoardManipulator.PiecePositionsAfterMove(piecePositions, sourceSquare, destinationSquare);

            if (ply.IsCapture)
            {
                var oppCol = ply.Colour == Colour.W ? 'B' : 'W';

                // update the opposing colour's piece position to remove the piece at the destination square
                foreach (var piece in Constants.PieceIndex.Keys)
                {

                    string oppPiecePositionsKey = new([oppCol, piece]);
                    currentBoardPosition.PiecePositions[oppPiecePositionsKey]
                        = bitBoardManipulator.RemovePiece(
                            currentBoardPosition.PiecePositions[oppPiecePositionsKey],
                            destinationSquare);
                }

                currentBoardPosition.PiecePositions[piecePositionsKey] = newPiecePositions;
            }

            currentBoardPosition.PiecePositions[piecePositionsKey] = newPiecePositions;
        }
    }
}
