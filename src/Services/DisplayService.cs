using System;
using Interfaces.DTO;

namespace Services
{
	public interface IDisplayService
	{
		sbyte[,] GetBoardArrayFromBoardPositions(BoardPosition boardPosition);
	}

    public class DisplayService : IDisplayService
    {
		public DisplayService()
		{
		}

        public sbyte[,] GetBoardArrayFromBoardPositions(BoardPosition boardPosition)
        {
            var returnValue = new sbyte[8,8];

            for (var pieceIndex = 0; pieceIndex < 12; pieceIndex++)
            {

            }

            return returnValue;
        }
    }

}

