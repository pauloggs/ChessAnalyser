using Interfaces;
using Interfaces.DTO;
using static Interfaces.Constants;

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
            Piece piece,
            Colour colour);

        /// <summary>
        /// Determine if the raw move specifies a source rank and/or file.
        /// For example, Nbd2 specifies the source file as 'B'
        /// </summary>
        (int specifiedRank, int specifiedFile) GetSourceRankAndOrFile(string rawMove);
    }

    public class SourceSquareHelper(IBitBoardManipulator bitBoardManipulator) : ISourceSquareHelper
    {
        public int GetSourceSquare(
            BoardPosition previousBoardPosition, 
            int potentialSourceRank, 
            int potentialSourceFile, 
            Piece piece, 
            Colour colour)
        {
            // default to not found
            var returnValue = MoveNotFound;

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

        public (int specifiedRank, int specifiedFile) GetSourceRankAndOrFile(string rawMove)
        {
            // Initialize to an "unspecified" or "invalid" state            
            int specifiedRank = -1;
            int specifiedFile = -1;

            // Remove promotion part if present
            string moveWithoutPromotion = rawMove;
            int promotionIndex = rawMove.IndexOf('=');
            if (promotionIndex != -1)
            {
                // Isolate the string *before* the equals sign
                moveWithoutPromotion = rawMove.Substring(0, promotionIndex);
            }

            var rawMoveWithoutCaptureOrPromotion = moveWithoutPromotion.Replace("x", "");

            // The last two characters are always the destination square (e.g., "d2")
            // Everything before that is the Piece designator + potential source disambiguation
            if (rawMoveWithoutCaptureOrPromotion.Length < 3)
            {
                // Not a piece move with enough info to disambiguate (e.g., "e4" or "Qh5")
                return (specifiedRank, specifiedFile);
            }

            // Moves will always end in the destination square (2 characters), so remove these to get the relevant part, e.g. Nf1e3 -> Nf1
            string relevantRawMove 
                = rawMoveWithoutCaptureOrPromotion[..^2];

            // The first character is always the piece designator (e.g., 'N' for Knight), so remove this, e.g. Nf1 -> f1
            string relevantRawMoveWithoutPiece 
                = relevantRawMove.Substring(1);

            // Check the characters in the disambiguation segment
            foreach (char c in relevantRawMoveWithoutPiece)
            {
                if (char.IsDigit(c) && c >= '1' && c <= '8')
                {
                    specifiedRank = c - '1'; // Convert char digit to 0-based index (0-7)
                }
                else if (char.IsLetter(c) && c >= 'a' && c <= 'h')
                {
                    specifiedFile = Constants.File[c]; // Convert file letter to 0-based index (0-7)
                }
            }

            return (specifiedRank, specifiedFile);
        }
    }
}
