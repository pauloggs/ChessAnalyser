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

        /// <summary>
        /// Moves a piece from sourceSquare to destinationSquare on the bitboard.
        /// Uses bitwise XOR to toggle the bits at the source and destination squares.
        /// </summary>
        /// <param name="currentPiecePositions"></param>
        /// <param name="sourceSquare"></param>
        /// <param name="destinationSquare"></param>
        /// <returns></returns>
        ulong MovePiece(
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

        public ulong MovePiece(
            ulong currentPiecePositions,
            int sourceSquare,
            int destinationSquare)
        {
            // Create a mask representing both the start and end squares
            // For example, if moving from A2 (square 8) to A3 (square 16)
            ulong moveMask = (1ul << sourceSquare) | (1ul << destinationSquare);

            // XOR toggles the bits: removes from 'from', adds to 'to'
            // For example, 0000...000100000000 (A2) XOR 0000...001000000000 (A3)
            var newPiecePositions = currentPiecePositions ^ moveMask;

            return newPiecePositions;
        }

        public ulong RemovePiece(
            ulong piecePositions,
            int square)
        {
            if (square < 0 || square > 63)
            {
                throw new ArgumentOutOfRangeException($"Square {square} is out of range when attempting to RemovePiece");
            }
                
            return piecePositions &= ~ (ulong)(1ul << square);
        }

        public ulong AddPiece(
            ulong piecePositions,
            int square)
        {
            if (square < 0 || square > 63)
            {
                throw new ArgumentOutOfRangeException($"Square {square} is out of range when attempting to RemovePiece");
            }

            return piecePositions |= (ulong)(1ul << square);
        }
    }    
}

