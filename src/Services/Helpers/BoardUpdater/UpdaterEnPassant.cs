using Interfaces.DTO;
using static Interfaces.Constants;

namespace Services.Helpers.BoardUpdater
{
    public interface IUpdaterEnPassant : IBoardUpdater
    {
    }

    public class UpdaterEnPassant(IBitBoardManipulator bitBoardManipulator) : IUpdaterEnPassant
    {
        public void UpdateBoard(
            BoardPosition currentBoardPosition,
            string piecePositionsKey,
            Ply ply,
            int sourceSquare,
            int destinationSquare)
        {
            // move the pawn
            var pawnPositions = currentBoardPosition.PiecePositions[piecePositionsKey];
            currentBoardPosition.PiecePositions[piecePositionsKey]
                = bitBoardManipulator.MovePiece(pawnPositions, sourceSquare, destinationSquare);

            // remove the captured pawn
            var oppCol = ply.Colour == Colour.W ? 'B' : 'W';
            string oppPawnPositionsKey = new([oppCol, 'P']);
            currentBoardPosition.PiecePositions[oppPawnPositionsKey]
                = bitBoardManipulator.RemovePiece(
                    currentBoardPosition.PiecePositions[oppPawnPositionsKey],
                    destinationSquare + (ply.Colour == Colour.W ? -8 : 8));
        }
    }
}
