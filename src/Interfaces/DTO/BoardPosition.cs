using System;
namespace Interfaces.DTO
{
    /// <summary>
    /// Comprises a 64bit Word (ulong) for each piece type and colour.
    /// </summary>
    public class BoardPosition
	{
		public Dictionary<string, ulong> PiecePositions { get; set; }

		public BoardPosition()
		{
			PiecePositions = new Dictionary<string, ulong>()
			{
				{ "WP", 0 },
				{ "WN", 0 },
				{ "WB", 0 },
				{ "WR", 0 },
				{ "WQ", 0 },
				{ "WK", 0 },
				{ "BP", 0 },
				{ "BN", 0 },
				{ "BB", 0 },
				{ "BR", 0 },
				{ "BQ", 0 },
				{ "BK", 0 }
            };
        }
	}
}

