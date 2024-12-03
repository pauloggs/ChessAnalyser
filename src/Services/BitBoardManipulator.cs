using Interfaces.DTO;

namespace Services
{
	public interface IBitBoardManipulator
	{
		bool readSquare(
            BoardPosition boardPosition,
            char piece,
            char colour,
            int square);
	}

    public class BitBoardManipulator : IBitBoardManipulator
    {
		public BitBoardManipulator()
		{
		}

        public bool readSquare(
            BoardPosition boardPosition,
            char piece,
            char colour,
            int square)
        {
            throw new NotImplementedException();
        }
    }
}

