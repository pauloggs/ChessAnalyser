using Interfaces;
using Interfaces.DTO;

namespace Services.Helpers
{
    public interface IDestinationSquareHelper
    {
        int GetDestinationSquare(Ply ply);
    }

    public class DestinationSquareHelper : IDestinationSquareHelper
    {
        public int GetDestinationSquare(Ply ply)
        {
            // For piece moves, the destination square is always the last two characters of the raw move
            if (ply.IsPieceMove)
            {
                try
                {
                    // Get destination file from second last character of raw move
                    ply.DestinationFile = Constants.File[ply.RawMove[^2]];
                }
                catch (Exception)
                {
                    throw;
                }

                // Get destination rank from last character of raw move
                ply.DestinationRank = (int)char.GetNumericValue(ply.RawMove[ply.RawMove.Length - 1]) - 1;

                // Return destination square as an integer 0-63
                return SquareHelper.GetSquareFromRankAndFile(ply.DestinationRank, ply.DestinationFile);
            }
            else if (ply.IsKingsideCastling || ply.IsQueensideCastling)
            {
                // Can't get single destination square for a castling move - this is handled elsewhere
                return -1;
            }
            else
            {
                throw new Exception("Move must be either a piece move or casteling.");
            }
        }
    }
}
