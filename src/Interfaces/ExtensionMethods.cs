using Interfaces.DTO;

namespace Interfaces
{
    public static class ExtensionMethods
    {
        public static BoardPosition DeepCopy(this BoardPosition boardPosition)
        {
            // 1. Guard clause ensures the method throws if the input is null.
            // In C# 8.0, the compiler's flow analysis sees this and knows
            // boardPosition is NOT null for the rest of the method.
            if (boardPosition is null)
            {
                throw new ArgumentNullException(nameof(boardPosition), "BoardPosition cannot be null.");
            }

            // 2. Creating a new object ensures a non-null reference is returned.
            var newBoardPosition = new BoardPosition
            {
                PiecePositions = new Dictionary<string, ulong>(boardPosition.PiecePositions),
                EnPassantTargetFile = boardPosition.EnPassantTargetFile
            };

            // 3. The compiler correctly treats this as a non-nullable return.
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

