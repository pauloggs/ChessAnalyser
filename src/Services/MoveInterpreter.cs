using Interfaces;
using Interfaces.DTO;

namespace Services
{
	public interface IMoveInterpreter
    {
        (Piece piece, int sourceSquare, int destinationSquare)
            GetSourceAndDestinationSquares(BoardPosition previousBoardPosition, string rawMove);
	}   

    /// <summary>
    /// From move Raf8, find out the following
    /// 1 the piece
    /// 2 the source square (0-63)
    /// 3 the destination square (0-63)
    /// </summary>
	public class MoveInterpreter : IMoveInterpreter
    {
        public (Piece piece, int sourceSquare, int destinationSquare)
            GetSourceAndDestinationSquares(BoardPosition previousBoardPosition, string rawMove)
        {
            if (string.IsNullOrEmpty(rawMove) || rawMove.Length < 2)
            {
                return (new Piece(), -1, -1);
            }

            var sourceSquare = -1;

            var destinationSquare = -1;

            rawMove = RemoveCheck(rawMove);

            var piece = GetPiece(rawMove);

            if (piece.Name != "Castling")
            {
                sourceSquare = GetSourceSquare(rawMove);

                destinationSquare = GetDestinationSquare(rawMove);
            };
            
            return (piece, sourceSquare, destinationSquare);
        }

        /// <summary>
        /// If the string contains '+' at the end, remove it.
        /// </summary>
        /// <param name="rawMove"></param>
        /// <returns></returns>
        private static string RemoveCheck(string rawMove)
        {
            if (rawMove[^1] == '+')
            {
                return rawMove.Remove(rawMove.Length - 1);
            }

            return rawMove;
        }

        private static Piece GetPiece(string rawMove)
        {
            Console.WriteLine($"MoveInt > GetPiece: '{rawMove}'");

            var firstChar = rawMove[0];

            Piece piece;

            if (firstChar == '0')
            {
                piece = Constants.Pieces['C']; // is a castling move
            }
            else if (char.IsLower(firstChar))
            {
                piece = Constants.Pieces['P']; // is a pawn move
            }
            else if (char.IsUpper(firstChar))
            {
                piece = Constants.Pieces[firstChar];
            }
            else
            {
                throw new Exception($"'{rawMove}' is invalid");
            }

            return piece;
        }

        private static int GetSourceSquare(string rawMove)
        {
            return 0;
        }

        private static int GetDestinationSquare(string rawMove)
        {
            var destinationFile = Constants.File[rawMove[^2]];

            var destinationRank = rawMove[^1];

            return destinationRank * 8 + (destinationFile - 1);
        }
    }
}

