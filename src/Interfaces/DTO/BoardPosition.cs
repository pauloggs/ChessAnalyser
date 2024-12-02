using System;
namespace Interfaces.DTO
{
    /// <summary>
    /// Comprises a 64bit Word (ulong) for each piece type and colour.
    /// </summary>
    public class BoardPosition
	{
		public ulong[] PiecePositions { get; set; }

		public BoardPosition()
		{
			// 0-5 are white pieces, 6 to 11 are black pieces
			// 0 / 6	P
			// 1 / 7	K
			// 2 / 8	B
			// 3 / 9	R
			// 4 / 10	Q
			// 5 / 11	K
			PiecePositions = new ulong[12];
		}
	}
}

