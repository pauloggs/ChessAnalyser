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
                = bitBoardManipulator.MovePiece(piecePositions, sourceSquare, destinationSquare);

            var oppCol = ply.Colour == Colour.W ? 'B' : 'W';

            // if the move is a capture, then make sure that there is an opposing pieceKeys at the destination square. If there isn't, throw an exception
            if (ply.IsCapture)
            {              
                // update the opposing colour's pieceKeys position to remove the pieceKeys at the destination square
                foreach (var pieceKey in Constants.PieceIndex.Keys)
                {

                    string oppPiecePositionsKey = new([oppCol, pieceKey]);
                    currentBoardPosition.PiecePositions[oppPiecePositionsKey]
                        = bitBoardManipulator.RemovePiece(
                            currentBoardPosition.PiecePositions[oppPiecePositionsKey],
                            destinationSquare);
                }
            }
            // if the move is not a capture, then make sure that there is no opposing pieceKeys at the destination square, if there is, throw an exception
            else
            {                 
                // check that there is no opposing pieceKeys at the destination square
                foreach (var pieceKeys in Constants.PieceIndex.Keys)
                {
                    string oppPiecePositionsKey = new([oppCol, pieceKeys]);
                    bool isOppPieceAtDestination
                        = bitBoardManipulator.ReadSquare(
                            currentBoardPosition,
                            ply.Piece,
                            oppCol == 'W' ? Colour.W : Colour.B,
                            destinationSquare / 8,
                            destinationSquare % 8);
                    if (isOppPieceAtDestination)
                    {
                        throw new InvalidOperationException($"There is an opposing piece at the destination square {destinationSquare} for a non-capture move.");
                    }
                }
            }


            currentBoardPosition.PiecePositions[piecePositionsKey] = newPiecePositions;
        }
    }
}
