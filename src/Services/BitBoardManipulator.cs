using Interfaces.DTO;
using Services.Helpers;
using static Interfaces.Constants;

namespace Services
{
    public interface IBitBoardManipulator
	{
        /// <summary>
        /// Determines whether a specific piece of a specific color occupies a precise square on the chessboard, 
        /// utilizing an underlying 64-bit bitboard representation for efficient lookup via bitwise operations.
        /// </summary>
        bool ReadSquare(
            BoardPosition boardPosition,
            Piece piece,
            Colour colour,
            int rank,
            int file);

        ulong PiecePositionsAfterMove(
            ulong piecePositions,
            int sourceSquare,
            int destinationSquare);

        ulong RemovePiece(
            ulong piecePositions,
            int square);

        ulong AddPiece(
            ulong piecePositions,
            int square);
    }

    public class BitBoardManipulator(IBitBoardManipulatorHelper bitBoardManipulatorHelper) : IBitBoardManipulator
    {
        public bool ReadSquare(
            BoardPosition boardPosition,
            Piece piece,
            Colour colour,
            int rank,
            int file)
        {
            if (rank < 0 || rank > 7 || file < 0 || file > 7)
            {
                throw new ArgumentOutOfRangeException($"Rank and file must be between 0 and 7 inclusive");
            }

            // Construct the key to access the piece positions
            // string piecePositionsKey = new(new[] { colour, piece });

            string piecePositionsKey = colour.ToString() + piece.Name;

            var piecePositionBytes
                = BitConverter.GetBytes(boardPosition.PiecePositions[piecePositionsKey])
                ?? [];

            if (piecePositionBytes.Length < 8)
            {
                throw new Exception($"Invalid bytes in board position");
            }

            return bitBoardManipulatorHelper.IsPiecePresentAtFileInRank(piecePositionBytes[rank], file);
        }


        ///// <summary>
        ///// Takes the supplied file and create a bitmask that is ANDed with the rank byte to determine if a piece exists on that file in the rank.
        ///// </summary>
        ///// <param name="fileByte"></param>
        ///// <param name="file"></param>
        ///// <returns></returns>
        //private static bool IsPiecePresentAtFileInRank(byte fileByte, int file)
        //{
        //    // Shift a 1 to the left by the file number and AND it with the rank byte
        //    return (fileByte & (1 << file)) != 0;
        //}

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

        public ulong AddPiece(
            ulong piecePositions,
            int square)
        {
            return piecePositions |= (ulong)(1ul << square);
        }
    }    
}

