using System;
using Interfaces;
using Interfaces.DTO;

namespace Services
{
	public interface IMoveInterpreter
    {
		(Piece piece, int sourceSquare, int destinationSquare) GetSourceAndDestinationSquares(string rawMove);
	}

    /// <summary>
    /// From move Raf8, find out the following
    /// 1 the piece
    /// 2 the source square (0-63)
    /// 3 the destination square (0-63)
    /// </summary>
	public class MoveInterpreter : IMoveInterpreter
    { 
        public (Piece piece, int sourceSquare, int destinationSquare) GetSourceAndDestinationSquares(string rawMove)
        {

            var sourceSquare = 0;
            var destinationSquare = 0;
            var piece = Constants.Pieces['X'];

            var firstChar = rawMove[0];

            if (string.IsNullOrEmpty(rawMove))
            {
                return (piece, -1, -1);
            }

            if (rawMove[rawMove.Length - 1] == '+')
            {
                // remove checks
                rawMove = rawMove.Remove(rawMove.Length - 1);
            }

            if (char.IsLower(firstChar))
            {
                // pawn move
                sourceSquare = rawMove[1];
                destinationSquare = GetFile(rawMove);
            }
            else if (char.IsUpper(firstChar))
            {
                // piece move
                piece = Constants.Pieces[firstChar];
                destinationSquare = GetFile(rawMove);
            }
            else if (firstChar == '0')
            {
                // castling move
            }
            else
            {
                throw new Exception($"'{rawMove}' is invalid");
            }

            return (piece, sourceSquare, destinationSquare);
        }

        private int GetFile(string rawMove)
        {
            // find the next to last character in the string - this is the desitnation file
            return rawMove[rawMove.Length - 2];
        }
    }
}

