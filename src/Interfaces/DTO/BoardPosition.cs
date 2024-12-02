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
			// 0-5 are white pieces, 6 to 11 are black pieces
			// 0 / 6	P
			// 1 / 7	K
			// 2 / 8	B
			// 3 / 9	R
			// 4 / 10	Q
			// 5 / 11	K
			//PiecePositions = new ulong[12];
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

