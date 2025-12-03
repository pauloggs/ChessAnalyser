using Interfaces.DTO;

namespace Services
{
    public interface IBitBoardManipulator
	{
        bool ReadSquare(
            BoardPosition boardPosition,
            char piece,
            char colour,
            int rank,
            int file);

        ulong PiecePositionsAfterMove(
            ulong piecePositions,
            int sourceSquare,
            int destinationSquare);

        ulong RemovePiece(
            ulong piecePositions,
            int square);
    }

    public class BitBoardManipulator : IBitBoardManipulator
    {

        public bool ReadSquare(
            BoardPosition boardPosition,
            char piece,
            char colour,
            int rank,
            int file)
        {
            string piecePositionsKey = new(new[] { colour, piece });

            var piecePositionBytes
                = BitConverter.GetBytes(boardPosition.PiecePositions[piecePositionsKey])
                ?? Array.Empty<byte>();

            if (piecePositionBytes.Length < 8)
            {
                throw new Exception($"Invalid bytes in board position");
            }

            return GetFileFromRank(piecePositionBytes[rank], file);
        }

        private static bool GetFileFromRank(byte files, int file)
        {
            return (files & (1 << file)) != 0;
        }

        public ulong PiecePositionsAfterMove(
            ulong piecePositions,
            int sourceSquare,
            int destinationSquare)
        {
            return piecePositions ^= ((ulong)(1ul << sourceSquare) + (ulong)(1ul << destinationSquare));
        }

        public ulong RemovePiece(
            ulong piecePositions,
            int square)
        {
            return piecePositions &= ~ (ulong)(1ul << square);
        }
    }    
}

