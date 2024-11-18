using Interfaces.DTO;

namespace Services
{
	public interface IBoardPositionGenerator
    {
        BoardPosition GetBoardPosition(BoardPosition previousBoardPosition, string rawMove);
    }

    /// <summary>
    /// Main function is to convert PGN moves, such as d4, Nf6 etc.
    /// into BoardPosition objects, based on the previous BoardPosition
    /// object.
    /// </summary>
    public class BoardPositionGenerator : IBoardPositionGenerator
    {
		public BoardPositionGenerator()
		{
		}

        public BoardPosition GetBoardPosition(BoardPosition previousBoardPosition, string rawMove)
        {
            throw new NotImplementedException();
        }
    }
}

