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
            var sourceSquare = -1;
            var destinationSquare = -1;
            var piece = Constants.Pieces['X'];

            if (string.IsNullOrEmpty(rawMove) || rawMove.Length < 2)
            {
                return (piece, -1, -1);
            }

            rawMove = RemoveCheck(rawMove);

            sourceSquare = GetSourceSquare(rawMove);

            destinationSquare = GetDestinationSquare(rawMove);

            return (piece, sourceSquare, destinationSquare);
        }


        private static int GetFile(string rawMove)
        {
            return rawMove[^2];
        }

        private static string RemoveCheck(string rawMove)
        {
            if (rawMove[^1] == '+')
            {
                return rawMove.Remove(rawMove.Length - 1);
            }

            return rawMove;
        }

        private int GetSourceSquare(string rawMove)
        {
            return 0;
        }

        private int GetDestinationSquare(string rawMove)
        {
            return 0;
        }
    }
}

