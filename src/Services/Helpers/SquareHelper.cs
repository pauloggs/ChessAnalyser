namespace Services.Helpers
{

    public static class SquareHelper
    {
        /// <summary>
        /// Get the square number (0-63) from the specified rank (0-7) and file (0-7).
        /// </summary>
        /// <param name="rank"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static int GetSquareFromRankAndFile(int rank, int file)
        {
            // validate inputs
            if (rank < 0 || rank > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(rank), "Rank must be between 0 and 7.");
            }

            // validate inputs
            if (file < 0 || file > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(file), "File must be between 0 and 7.");
            }

            // calculate and return square number
            return (rank * 8) + file;
        }
    }
}
