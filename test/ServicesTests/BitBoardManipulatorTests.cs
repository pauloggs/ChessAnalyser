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
        // This is how you'd structure the test function once the data source is defined below
        public void ReadSquare_ShouldReturnTrueForOccupiedSquare(
            Colour colour, char piece, int rank, int file)
        {
            // Arrange
            BoardPosition board = CreateBoardWithPiece(colour, piece, rank, file);

            // Act
            // Use the name of whichever method you are currently testing (original or optimized)
            bool result = sut.ReadSquare(board, piece, colour, rank, file);

            // Assert
            Assert.True(result, $"Expected {colour} {piece} at Rank {rank}, File {file} to be present.");
        }

        [Theory]
        [InlineData(Colour.W, 'P', 0, 0, 7, 7)] // Check 'a1' when piece is at 'h8'
        // ... define more empty square tests below ...
        public void ReadSquare_ShouldReturnFalseForEmptySquare(
            Colour pieceColour, char pieceType, int placedRank, int placedFile,
            int checkedRank, int checkedFile)
        {
            // Arrange
            // Place one piece somewhere specific
            BoardPosition board = CreateBoardWithPiece(pieceColour, pieceType, placedRank, placedFile);

            // Act
            // Check a square that is definitively empty
            bool result = sut.ReadSquare(board, pieceType, pieceColour, checkedRank, checkedFile);

            // Assert
            Assert.False(result, "Expected the checked square to be empty.");
        }

        [Fact]
        public void ReadSquare_ShouldThrowExceptionForInvalidPieceKey()
        {
            // Arrange
            var board = new BoardPosition { PiecePositions = new Dictionary<string, ulong>() };

            Assert.Throws<KeyNotFoundException>(() => sut.ReadSquare(board, 'Q', Colour.B, 0, 0));
        }

        // A comprehensive MemberData source ensures all positions are covered for basic occupancy
        public static IEnumerable<object[]> GetPiecePlacementTestData()
        {
            // Test White Pawns
            yield return new object[] { Colour.W, 'P', 1, 0 }; // White Pawn at a2
            yield return new object[] { Colour.W, 'P', 6, 7 }; // White Pawn at h7 (unusual place for start)

            // Test Black Knights
            yield return new object[] { Colour.B, 'N', 7, 1 }; // Black Knight at b8
            yield return new object[] { Colour.B, 'N', 0, 6 }; // Black Knight at g1 (unusual)

            // Test Queens (Edge cases of board)
            yield return new object[] { Colour.W, 'Q', 0, 0 }; // White Queen at a1
            yield return new object[] { Colour.B, 'Q', 7, 7 }; // Black Queen at h8

            // Test a middle square
            yield return new object[] { Colour.W, 'R', 3, 4 }; // White Rook at e4
        }

        [Fact]
        public void ReadSquare_ShouldReturnTrueForMultipleOccupiedSquaresOfSameType()
        {
            // Arrange: Place two white pawns simultaneously on a2 and b2 (rank 1, files 0 and 1)

            int rank = 1;
            int file1 = 0; // a2
            int file2 = 1; // b2
            Colour colour = Colour.W;
            char piece = 'P';

            ulong mask1 = 1UL << ((rank * 8) + file1);
            ulong mask2 = 1UL << ((rank * 8) + file2);
            ulong combinedMask = mask1 | mask2; // Use bitwise OR to set both bits

            var board = new BoardPosition();
            board.PiecePositions = new Dictionary<string, ulong>
            {
                { $"{colour}{piece}", combinedMask }
            };

            // Act & Assert
            bool result1 = sut.ReadSquare(board, piece, colour, rank, file1);
            bool result2 = sut.ReadSquare(board, piece, colour, rank, file2);
            bool result3 = sut.ReadSquare(board, piece, colour, 1, 2); // Check c2 (empty)

            Assert.True(result1, "Should find pawn at a2.");
            Assert.True(result2, "Should find pawn at b2.");
            Assert.False(result3, "Should not find pawn at c2 (empty square check).");
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
            bool checkWp = sut.ReadSquare(board, 'P', Colour.W, rank, file);
            // Check for the Black Knight at e4
            bool checkBN = sut.ReadSquare(board, 'N', Colour.B, rank, file);
            // Check for a White Knight at e4 (should be false)
            bool checkWN = sut.ReadSquare(board, 'N', Colour.W, rank, file);


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

            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => sut.ReadSquare(board, 'P', Colour.W, rank, file));
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
