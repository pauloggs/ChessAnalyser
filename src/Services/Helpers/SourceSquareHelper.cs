using Interfaces;
using Interfaces.DTO;

namespace Services.Helpers
{
    public interface ISourceSquareHelper
    {
        /// <summary>
        /// Check if the specified colour/piece is at the specified rank/file.
        /// Return the 0-63 square number if present, or -1 if not.
        /// </summary>
        /// <param name="previousBoardPosition"></param>
        /// <param name="potentialSourceRank"></param>
        /// <param name="potentialSourceFile"></param>
        /// <param name="piece"></param>
        /// <param name="colour"></param>
        /// <returns></returns>
        int GetSourceSquare(
            BoardPosition previousBoardPosition,
            int potentialSourceRank,
            int potentialSourceFile,
            char piece,
            char colour);
    }

    public class SourceSquareHelper(IBitBoardManipulator bitBoardManipulator) : ISourceSquareHelper
    {
        int ISourceSquareHelper.GetSourceSquare(BoardPosition previousBoardPosition, int potentialSourceRank, int potentialSourceFile, char piece, char colour)
        {
            // default to not found
            var returnValue = -1;

            if (potentialSourceFile >= 0 && potentialSourceFile < 8
                 && potentialSourceRank >= 0 && potentialSourceRank < 8)
            {
                // check if the piece is here
                var potentialKnightSquare = bitBoardManipulator.ReadSquare(
                    previousBoardPosition,
                    piece,
                    colour,
                    potentialSourceRank,
                    potentialSourceFile);

                if (potentialKnightSquare == true)
                {
                    returnValue = SquareHelper.GetSquareFromRankAndFile(potentialSourceRank, potentialSourceFile);
                }
            }

            return returnValue;
        }
    }
}
