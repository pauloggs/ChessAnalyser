using System;
using System.Text;
using Interfaces.DTO;

namespace Services
{
	public interface IDisplayService
	{
        void DisplayBoardPosition(sbyte[,] boardArray);

		sbyte[,] GetBoardArrayFromBoardPositions(BoardPosition boardPosition);
	}

    public class DisplayService : IDisplayService
    {
        public void DisplayBoardPosition(sbyte[,] boardArray)
        {
        }

        public sbyte[,] GetBoardArrayFromBoardPositions(BoardPosition boardPosition)
        {
            var boardArray = new sbyte[8,8];

            foreach (var key in boardPosition.PiecePositions.Keys)
            {
                var col = key[0];

                var piecePositions = boardPosition.PiecePositions[key];

                var piecePositionsToString = piecePositions.ToString();

                byte[] mybyt = BitConverter.GetBytes(piecePositions);

                var b = Encoding.ASCII.GetBytes(piecePositionsToString);
            }

            return boardArray;
        }

        
    }
}

