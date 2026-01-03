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
        /// <summary>
        /// Updates the board position based on the supplied ply details.
        /// If the move is a capture, it ensures that an opposing piece exists at the destination square.
        /// If the move is not a capture, it ensures that no opposing piece exists at the destination square.
        /// </summary>
        public void UpdateBoard(
            BoardPosition boardPosition,
            string piecePositionsKey, // e.g., "WP" for White Pawn
            Ply ply,
            int sourceSquare,
            int destinationSquare)
        {
            var piecePositions = boardPosition.PiecePositions[piecePositionsKey];

            var newPiecePositions
                = bitBoardManipulator.MovePiece(piecePositions, sourceSquare, destinationSquare);

            var oppCol = ply.Colour == Colour.W ? 'B' : 'W';

            // if the move is a capture, then make sure that there is an opposing pieceKey at the destination square. If there isn't, throw an exception
            if (ply.IsCapture)
            {
                // update the opposing colour's pieceKey position to remove the pieceKey at the destination square
                foreach (var pieceKey in Constants.PieceIndex.Keys)
                {

                    string oppPiecePositionsKey = new([oppCol, pieceKey]);

                    bool isOppPieceAtDestination
                        = bitBoardManipulator.ReadSquare(
                            boardPosition,
                            ply.Piece,
                            oppCol == 'W' ? Colour.W : Colour.B,
                            destinationSquare / 8,
                            destinationSquare % 8);

                    if (isOppPieceAtDestination)
                    {
                        throw new InvalidOperationException($"There is no opposing piece at the destination square {destinationSquare} for a capture move.");
                    }

                    boardPosition.PiecePositions[oppPiecePositionsKey]
                        = bitBoardManipulator.RemovePiece(
                            boardPosition.PiecePositions[oppPiecePositionsKey],
                            destinationSquare);
                }
            }
            // if the move is not a capture, then make sure that there is no opposing pieceKey at the destination square, if there is, throw an exception
            else
            {
                var (occupyingPiece, _) = bitBoardManipulator.ReadSquare(
                    boardPosition,
                    destinationSquare);

                if (occupyingPiece != null)
                {
                    throw new InvalidOperationException($"There is an opposing piece at the destination square {destinationSquare} for a non-capture move.");
                }


                //    // check that there is no opposing pieceKey at the destination square
                //    foreach (var pieceKey in Constants.PieceIndex.Keys)
                //{
                //    string oppPiecePositionsKey = new([oppCol, pieceKey]);
                //    var oppPiece = Constants.Pieces[pieceKey];
                //    bool isOppPieceAtDestination
                //        = bitBoardManipulator.ReadSquare(
                //            boardPosition,
                //            oppPiece,
                //            oppCol == 'W' ? Colour.W : Colour.B,
                //            destinationSquare / 8,
                //            destinationSquare % 8);
                //    if (isOppPieceAtDestination)
                //    {
                //        throw new InvalidOperationException($"There is an opposing piece at the destination square {destinationSquare} for a non-capture move.");
                //    }
            }
        }

        //currentBoardPosition.PiecePositions[piecePositionsKey] = newPiecePositions;
    }
}