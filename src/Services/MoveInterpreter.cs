using System;
using Interfaces;
using Interfaces.DTO;

namespace Services
{
	public interface IMoveInterpreter
    {
		(Piece piece, int rank, int file) GetDestinationRankAndFile(string rawMove);
	}

	public class MoveInterpreter : IMoveInterpreter
    { 
        public (Piece piece, int rank, int file) GetDestinationRankAndFile(string rawMove)
        {

            var rank = 0;
            var file = 0;
            var piece = Constants.Pieces['P'];

            var firstChar = rawMove[0];

            if (char.IsLower(firstChar))
            {
                // pawn move
                rank = rawMove[1];
                file = GetFile(rawMove);
            }
            else if (char.IsUpper(firstChar))
            {
                // piece move
                piece = Constants.Pieces[firstChar];
                file = GetFile(rawMove);
            }
            else if (firstChar == '0')
            {
                // castling move
            }
            else
            {
                throw new Exception($"'{rawMove}' is invalid");
            }

            return (piece, rank, file);
        }

        private int GetFile(string rawMove)
        {
            // find the next to last character in the string - this is the desitnation file
            return rawMove[rawMove.Length - 2];
        }
    }
}

