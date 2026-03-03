using Interfaces.DTO;

namespace Interfaces
{
    public static class ExtensionMethods
    {
        public static BoardPosition DeepCopy(this BoardPosition boardPosition)
        {
            if (boardPosition is null)
            {
                throw new ArgumentNullException(nameof(boardPosition), "BoardPosition cannot be null.");
            }

            var newBoardPosition = new BoardPosition
            {
                PiecePositions = new Dictionary<string, ulong>(boardPosition.PiecePositions),
                EnPassantTargetFile = boardPosition.EnPassantTargetFile
            };

            return newBoardPosition;
        }

        public static int GetSquareFromRankAndFile(int rank, int file)
        {
            return rank * 8 + file;
        }

        public static string Algebraic(this int value)
        {
            var file = (char)('a' + (value % 8));
            var rank = (value / 8) + 1;
            return $"{file}{rank}";
        }
    }
}

