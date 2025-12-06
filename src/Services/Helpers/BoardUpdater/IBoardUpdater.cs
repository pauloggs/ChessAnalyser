using Interfaces.DTO;

namespace Services.Helpers.BoardUpdater
{
    public interface IBoardUpdater
    {
        void UpdateBoard(
            BoardPosition currentBoardPosition,
            string piecePositionsKey,
            Ply ply,
            int sourceSquare,
            int destinationSquare);
    }
}
