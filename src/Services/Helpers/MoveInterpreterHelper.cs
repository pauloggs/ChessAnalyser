using Interfaces;
using Interfaces.DTO;

namespace Services.Helpers
{
    public static class MoveInterpreterHelper
    {
        /// <summary>
        /// Removes the check indicator '+' from the raw move string of the given ply.
        /// </summary>
        /// <param name="ply"></param>
        public static void RemoveCheck(Ply ply)
        {
            if (ply.RawMove[^1] == '+')
            {
                ply.RawMove = ply.RawMove.Remove(ply.RawMove.Length - 1);
                ply.IsCheck = true;
            }
        }

        /// <summary>
        /// Gets the piece involved in the given ply based on its raw move string.
        /// </summary>
        /// <param name="ply"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Piece GetPiece(Ply ply)
        {
            var rawMove = ply.RawMove;

            var firstChar = rawMove[0];

            Piece piece;

            if (rawMove.ToLower().Contains('x'))
            {
                ply.IsCapture = true;
            }

            if (rawMove.Contains('='))
            {
                ply.IsPawnMove = true;
                ply.IsPromotion = true;
                var promotionPiece = (rawMove.Substring(rawMove.IndexOf('=') + 1))[0];
                piece = PieceRetriever.GetSafePiece(promotionPiece); // get the promotion piece safely
            }
            else if (rawMove == "O-O-O")
            {
                piece = Constants.Pieces['C']; // is a castling move
                ply.IsQueensideCastling = true;
            }
            else if (rawMove == "O-O")
            {
                piece = Constants.Pieces['C']; // is a castling move
                ply.IsKingsideCastling = true;
            }
            else if (char.IsLower(firstChar))
            {
                piece = Constants.Pieces['P']; // is a pawn move
                ply.IsPawnMove = true;
                ply.IsPieceMove = true;
                ply.Piece = 'P';
            }
            else if (char.IsUpper(firstChar))
            {
                piece = PieceRetriever.GetSafePiece(firstChar); // get the piece safely
                ply.IsPieceMove = true;
                ply.Piece = firstChar;
            }
            else
            {
                throw new Exception($"'{rawMove}' is invalid");
            }

            return piece;
        }

        /// <summary>
        /// Get the destination square (0-63) from the last two characters of
        /// the move, for example Nf6 takes 'f' and '6' to determine file and
        /// rank, and from these determine the destination square.
        /// If it is not a piece move (i.e. is castling), return -1.
        /// </summary>
        public static int GetDestinationSquare(Ply ply)
        {
            if (ply.IsPieceMove)
            {
                try
                {
                    ply.DestinationFile = Constants.File[ply.RawMove[^2]];
                }
                catch (Exception)
                {
                    throw;
                }

                ply.DestinationRank = (int)char.GetNumericValue(ply.RawMove[ply.RawMove.Length - 1]) - 1;

                return (ply.DestinationRank * 8) + ply.DestinationFile;
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
