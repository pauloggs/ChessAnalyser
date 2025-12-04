using Interfaces;
using Interfaces.DTO;
using Services;
using static Interfaces.Constants;

namespace ServicesTests
{
    public class BitBoardManipulatorTests
    {
        private readonly IBitBoardManipulator sut;
        public BitBoardManipulatorTests()
        {
            sut = new BitBoardManipulator();
        }

        [Theory]
        [MemberData(nameof(GetPiecePlacementTestData))]
        public void ReadSquare_ShouldReturnTrueForOccupiedSquare(
            Colour colour, Piece piece, int rank, int file)
        {
            // Arrange
            BoardPosition board = CreateBoardWithPiece(colour, piece.Name, rank, file);

            // Act
            // Use the name of whichever method you are currently testing (original or optimized)
            bool result = sut.ReadSquare(board, piece, colour, rank, file);

            // Assert
            Assert.True(result, $"Expected {colour} {piece} at Rank {rank}, File {file} to be present.");
        }

        [Theory]
        [InlineData(Colour.W, 'P', 0, 0, 7, 7)] // Check 'a1' when piece is at 'h8'
        [InlineData(Colour.B, 'N', 7, 1, 0, 6)] // Check 'g1' when piece is at 'b8'
        [InlineData(Colour.W, 'Q', 3, 3, 4, 4)] // Check 'e5' when piece is at 'd4'
        public void ReadSquare_ShouldReturnFalseForEmptySquare(
            Colour pieceColour, char pieceType, int placedRank, int placedFile,
            int checkedRank, int checkedFile)
        {
            // Arrange
            // Place one piece somewhere specific
            BoardPosition board = CreateBoardWithPiece(pieceColour, pieceType, placedRank, placedFile);

            // Act
            // Check a square that is definitively empty
            bool result = sut.ReadSquare(board, Constants.Pieces[pieceType], pieceColour, checkedRank, checkedFile);

            // Assert
            Assert.False(result, "Expected the checked square to be empty.");
        }

        [Fact]
        public void ReadSquare_ShouldThrowExceptionForInvalidPieceKey()
        {
            // Arrange
            var board = new BoardPosition { PiecePositions = new Dictionary<string, ulong>() };

            Assert.Throws<KeyNotFoundException>(() => sut.ReadSquare(board, Constants.Pieces['Q'], Colour.B, 0, 0));
        }

        // A comprehensive MemberData source ensures all positions are covered for basic occupancy
        public static IEnumerable<object[]> GetPiecePlacementTestData()
        {
            // Test White Pawns
            yield return new object[] { Colour.W, Constants.Pieces['P'], 1, 0 }; // White Pawn at a2
            yield return new object[] { Colour.W, Constants.Pieces['P'], 6, 7 }; // White Pawn at h7 (unusual place for start)

            // Test Black Knights
            yield return new object[] { Colour.B, Constants.Pieces['N'], 7, 1 }; // Black Knight at b8
            yield return new object[] { Colour.B, Constants.Pieces['N'], 0, 6 }; // Black Knight at g1 (unusual)

            // Test Queens (Edge cases of board)
            yield return new object[] { Colour.W, Constants.Pieces['Q'], 0, 0 }; // White Queen at a1
            yield return new object[] { Colour.B, Constants.Pieces['Q'], 7, 7 }; // Black Queen at h8

            // Test a middle square
            yield return new object[] { Colour.W, Constants.Pieces['R'], 3, 4 }; // White Rook at e4
        }

        [Fact]
        public void ReadSquare_ShouldDistinguishBetweenDifferentPieceTypesAndColors()
        {
            // Arrange:
            // Put a White Pawn on e4 and a Black Knight on e4 (conceptually overlapping for test setup)
            int rank = 3; // Rank 4
            int file = 4; // File e

            ulong mask = 1UL << ((rank * 8) + file);

            var board = new BoardPosition
            {
                PiecePositions = new Dictionary<string, ulong>
                {
                    { $"{Colour.W}{'P'}", mask }, // Key "WP"
                    { $"{Colour.B}{'N'}", mask }, // Key "BN"
                    { $"{Colour.W}{'N'}", 0UL },         
                    { $"{Colour.B}{'P'}", 0UL },
                    { $"{Colour.W}{'R'}", 0UL },
                }
            };

            // Act & Assert
            // Check for the White Pawn at e4
            bool checkWp = sut.ReadSquare(board, Constants.Pieces['P'], Colour.W, rank, file);
            // Check for the Black Knight at e4
            bool checkBN = sut.ReadSquare(board, Constants.Pieces['N'], Colour.B, rank, file);
            // Check for a White Knight at e4 (should be false)
            bool checkWN = sut.ReadSquare(board, Constants.Pieces['N'], Colour.W, rank, file);


            Assert.True(checkWp, "Should find the white pawn at e4.");
            Assert.True(checkBN, "Should find the black knight at e4.");
            Assert.False(checkWN, "Should not find a white knight at e4 (wrong piece/color combo).");
        }


        [Theory]
        [InlineData(8, 0)] // Rank 8 (out of bounds)
        [InlineData(-1, 0)] // Rank -1 (out of bounds)
        [InlineData(0, 8)] // File 8 (out of bounds)
        [InlineData(0, -1)] // File -1 (out of bounds)
        public void ReadSquare_ShouldThrowExceptionForInvalidCoordinates(int rank, int file)
        {
            // Arrange
            BoardPosition board = CreateBoardWithPiece(Colour.W, 'P', 1, 1);

            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => sut.ReadSquare(board, Constants.Pieces['P'], Colour.W, rank, file));
        }


        // Helper method to create a BoardPosition for testing
        // This method assumes the BoardPosition class and Colour enum exist.
        private BoardPosition CreateBoardWithPiece(Colour colour, char piece, int rank, int file)
        {
            // The bit manipulation we discussed: Set the specific bit for the location
            // The target bit index is (rank * 8) + file
            ulong pieceMask = 1UL << ((rank * 8) + file);

            var bp = new BoardPosition();
            bp.PiecePositions = new Dictionary<string, ulong>
            {
                // Set up the board with only the target piece
                { $"{colour}{piece}", pieceMask },
                // Add other keys for completeness if necessary, but keep them empty (0)
                { "WhiteP", 0UL },
                { "BlackN", 0UL }
            };
            return bp;
        }
    }
}
