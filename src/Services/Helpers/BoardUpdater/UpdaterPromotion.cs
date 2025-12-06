using Interfaces;
using Interfaces.DTO;
using static Interfaces.Constants;

namespace Services.Helpers.BoardUpdater
{
    public interface IUpdaterPromotion : IBoardUpdater
    {
    }

    public class UpdaterPromotion(IBitBoardManipulator bitBoardManipulator) : IUpdaterPromotion
    {
        public void UpdateBoard(
            BoardPosition currentBoardPosition,
            string piecePositionsKey,
            Ply ply,
            int sourceSquare,
            int destinationSquare)
        {
            // remove the pawn from the source square
            var pawnPositions = currentBoardPosition.PiecePositions[piecePositionsKey];
            currentBoardPosition.PiecePositions[piecePositionsKey]
                = bitBoardManipulator.RemovePiece(pawnPositions, sourceSquare);

            // add the promoted piece to the destination square
            string promotedPiecePositionsKey = ply.Colour.ToString() + ply.PromotionPiece.Name;
            var promotedPiecePositions = currentBoardPosition.PiecePositions[promotedPiecePositionsKey];
            currentBoardPosition.PiecePositions[promotedPiecePositionsKey]
                = bitBoardManipulator.AddPiece(promotedPiecePositions, destinationSquare);

            // if it's a capture, remove the opponent's piece from the destination square
            if (ply.IsCapture)
            {
                var oppCol = ply.Colour == Colour.W ? 'B' : 'W';
                foreach (var piece in Constants.PieceIndex.Keys)
                {
                    string oppPiecePositionsKey = new([oppCol, piece]);
                    currentBoardPosition.PiecePositions[oppPiecePositionsKey]
                        = bitBoardManipulator.RemovePiece(
                            currentBoardPosition.PiecePositions[oppPiecePositionsKey],
                            destinationSquare);
                }
            }
        }
    }
}
