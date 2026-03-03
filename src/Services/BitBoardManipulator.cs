using Interfaces;
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
        /// Returns the piece at the given square, and its colour.
        /// Throws an exception if the square is out of range.
        /// Throws an exception is more than one piece is found at the given square.
        /// </summary>
        (Piece? piece, Colour? colour) ReadSquare(
            BoardPosition boardPosition,
            int square);

        /// <summary>
        /// Moves a piece on the board according to the specified ply.
        /// </summary>
        /// <remarks>This method updates the board state to reflect the move described by <paramref
        /// name="ply"/>. The caller is responsible for ensuring that <paramref name="ply"/> is valid for the given
        /// <paramref name="boardPosition"/>.</remarks>
        /// <param name="boardPosition">The current position of all pieces on the board. Must not be null.</param>
        /// <param name="ply">The move to apply to the board position. Must represent a legal move in the current position.</param>
        void MovePiece(
            BoardPosition boardPosition,
            Ply ply);

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

        /// <summary>
        /// Removes any piece of the given colour from the given square in the BoardPosition.
        /// </summary>
        /// <param name="boardPosition"></param>
        /// <param name="square"></param>
        /// <param name="colour"></param>
        /// <returns></returns>
        void RemovePiece(
            BoardPosition boardPosition,
            int square,
            Colour colour);

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

        public (Piece? piece, Colour? colour) ReadSquare(
            BoardPosition boardPosition,
            int square // 0-63
            )
        {
            var occupyingPieceKey = 'X';

            var occupyingColour = Colour.N;

            var occupyingPieceCount = 0;

            var actualColours = Enum.GetValues<Colour>().Where(c => c != Colour.N);

            foreach (var colour in actualColours)
            {
                foreach (var pieceKey in Constants.PieceIndex.Keys)
                {
                    var piecePositionsKey = colour.ToString() + pieceKey;
                    var piecePositions = boardPosition.PiecePositions[piecePositionsKey];

                    if ((piecePositions & (1ul << square)) != 0)
                    {
                        occupyingPieceCount++;
                        if (occupyingPieceCount > 1)
                        {
                            throw new InvalidOperationException($"More than one piece found at square {square.Algebraic()}");
                        }
                        occupyingPieceKey = pieceKey;
                        occupyingColour = colour;
                    }
                }
            }
            
            if (occupyingPieceCount == 0)
            {
                return (null, null); // No piece found at the square
            }

            return (Constants.Pieces[occupyingPieceKey], occupyingColour);
        }

        public void MovePiece(
            BoardPosition boardPosition,
            Ply ply)
        {
            var sourceSquare = ply.SourceSquare;
            var destinationSquare = ply.DestinationSquare;

            var piecePositionsKey = ply.PiecePositionsKey;

            var currentPiecePositions = boardPosition.PiecePositions[piecePositionsKey];

            // Create a mask representing both the start and end squares
            // For example, if moving from A2 (square 8) to A3 (square 16)
            ulong moveMask = (1ul << sourceSquare) | (1ul << destinationSquare);

            // XOR toggles the bits: removes from 'from', adds to 'to'
            // For example, 0000...000100000000 (A2) XOR 0000...001000000000 (A3)
            var newPiecePositions = currentPiecePositions ^ moveMask;

            boardPosition.PiecePositions[piecePositionsKey] = newPiecePositions;
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

        public void RemovePiece(
            BoardPosition boardPosition,
            int square,
            Colour colour)
        {
            if (square < 0 || square > 63)
            {
                throw new ArgumentOutOfRangeException($"Square {square} is out of range when attempting to RemovePiece");
            }

            foreach (var pieceKey in Constants.PieceIndex.Keys)
            {
                var piecePositionsKey = colour.ToString() + pieceKey;
                var piecePositions = boardPosition.PiecePositions[piecePositionsKey];
                piecePositions &= ~(ulong)(1ul << square);
                boardPosition.PiecePositions[piecePositionsKey] = piecePositions;
            }
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

