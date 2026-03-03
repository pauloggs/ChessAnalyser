namespace Services.Helpers
{
    public interface IBitBoardManipulatorHelper
    {
        /// <summary>
        /// Takes the supplied file and create a bitmask that is ANDed with 
        /// the rank byte to determine if a piece exists on that file in the rank.
        /// </summary>
        /// <param name="fileByte"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        bool IsPiecePresentAtFileInRank(byte rankOccupancy, int file);
    }

    public class BitBoardManipulatorHelper : IBitBoardManipulatorHelper
    {
        public bool IsPiecePresentAtFileInRank(byte ranks, int file)
        {
            if (file < 0 || file > 7)
            {
                throw new ArgumentOutOfRangeException($"Invalid file provided {file}");
            }

            // Shift a 1 to the left by the file number and AND it with the rank byte
            return (ranks & (1 << file)) != 0;
        }
    }
}
