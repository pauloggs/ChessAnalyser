using Interfaces;
using Interfaces.DTO;

namespace Services.Helpers
{
    public static class PieceRetriever
    {
        public static Piece GetSafePiece(char key)
        {
            try
            {
                return Constants.Pieces[key];
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to retrieve Piece from key: '{key}'", ex);
            }
        }
    }
}
