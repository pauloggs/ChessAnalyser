using System;
namespace Interfaces.DTO
{
    /// <summary>
    /// Comprises a 64bit Word (ulong) sfor each piece type and colour.
    /// </summary>
    public class BoardPosition
	{
		public ulong[] Pawns { get; set; }
		public ulong[] Knights { get; set; }
		public ulong[] Bishops { get; set; }
		public ulong[] Rooks { get; set; }
		public ulong[] Queens { get; set; }
		public ulong[] Kings { get; set; }

		public BoardPosition()
		{
			Pawns = new ulong[2]; // 0 = white, 1 = blacks
			Knights = new ulong[2];
			Bishops = new ulong[2];
			Rooks = new ulong[2];
			Queens = new ulong[2];
			Kings = new ulong[2];
		}
	}
}

