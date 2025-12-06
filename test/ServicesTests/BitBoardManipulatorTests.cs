using Interfaces;
using Interfaces.DTO;
using Moq;
using Services;
using Services.Helpers;
using static Interfaces.Constants;

namespace ServicesTests
{
    public class BitBoardManipulatorTests
    {
        private readonly Mock<IBitBoardManipulatorHelper> bitBoardManipulatorHelperMock;
        private readonly IBitBoardManipulator sut;
        public BitBoardManipulatorTests()
        {
            bitBoardManipulatorHelperMock = new Mock<IBitBoardManipulatorHelper>(); 
            sut = new BitBoardManipulator(bitBoardManipulatorHelperMock.Object);
        }

        [Theory]
        [MemberData(nameof(GetPiecePlacementTestData))]
        public void ReadSquare_ShouldReturnTrueForOccupiedSquare(
            Colour colour, Piece piece, int rank, int file)
        {
            // Arrange
            BoardPosition board = CreateBoardWithPiece(colour, piece.Name, rank, file);

            var bitBoardManipulatorHelpere = new BitBoardManipulatorHelper();
            var bitBoardManipulator = new BitBoardManipulator(bitBoardManipulatorHelpere);

            // Act
            // Use the name of whichever method you are currently testing (original or optimized)
            bool result = bitBoardManipulator.ReadSquare(board, piece, colour, rank, file);

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

            var bitBoardManipulatorHelpere = new BitBoardManipulatorHelper();
            var bitBoardManipulator = new BitBoardManipulator(bitBoardManipulatorHelpere);

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
            bool checkWp = bitBoardManipulator.ReadSquare(board, Constants.Pieces['P'], Colour.W, rank, file);
            // Check for the Black Knight at e4
            bool checkBN = bitBoardManipulator.ReadSquare(board, Constants.Pieces['N'], Colour.B, rank, file);
            // Check for a White Knight at e4 (should be false)
            bool checkWN = bitBoardManipulator.ReadSquare(board, Constants.Pieces['N'], Colour.W, rank, file);


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


        [Theory(DisplayName = "Should remove a piece (turn off a bit) at a specific square")]
        // Test removing a piece from various positions when only that piece is present
        [InlineData(0b1ul, 0, 0b0ul)]                          // Remove A1 piece (LSB)
        [InlineData(0b10ul, 1, 0b0ul)]                         // Remove B1 piece
        [InlineData(0b10000000ul, 7, 0b0ul)]                  // Remove H1 piece
        [InlineData(0b10000000000000000000000000000000ul, 31, 0b0ul)] // Remove D4 piece (mid-board)
        [InlineData(0b1000000000000000000000000000000000000000000000000000000000000000ul, 63, 0b0ul)] // Remove H8 piece (MSB)
        public void RemovePiece_WhenOnlyTargetPieceExists_ReturnsZero(ulong initialPosition, int squareToRemove, ulong expectedResult)
        {
            // Act
            ulong actual = sut.RemovePiece(initialPosition, squareToRemove);

            // Assert
            Assert.Equal(expectedResult, actual);
        }

        [Fact(DisplayName = "Should remove the target piece while leaving other pieces intact")]
        public void RemovePiece_WithMultiplePieces_OnlyRemovesTargetPiece()
        {
            // Board state with pieces at A1 (0), D1 (3), A2 (8), H8 (63)
            ulong initialPosition = 0b10000000_00000000_00000000_00000000_00000000_00000000_00001001ul;

            // Expected board state after removing the piece at square 3 (D1)
            ulong expectedResult = 0b10000000_00000000_00000000_00000000_00000000_00000000_00000001ul;

            // Act: Remove the piece at D1 (square 3)
            ulong actual = sut.RemovePiece(initialPosition, 3);

            // Assert
            Assert.Equal(expectedResult, actual);

            // Verify that removing an already non-existent piece doesn't change anything
            ulong actualNoChange = sut.RemovePiece(actual, 3);
            Assert.Equal(expectedResult, actualNoChange);
        }

        [Fact(DisplayName = "Removing a piece from an empty board should result in an empty board")]
        public void RemovePiece_FromEmptyBoard_RemainsEmpty()
        {
            ulong emptyBoard = 0b0ul;
            int squareToRemove = 32; // Any valid square

            ulong actual = sut.RemovePiece(emptyBoard, squareToRemove);

            Assert.Equal(emptyBoard, actual);
        }

        [Fact(DisplayName = "Removing a piece from a full board should result in a board with one empty square")]
        public void RemovePiece_FromFullBoard_LeavesOneEmptySquare()
        {
            ulong fullBoard = ulong.MaxValue; // All 64 bits set to 1
            int squareToRemove = 0; // Remove A1 piece

            ulong expectedResult = ulong.MaxValue ^ (1ul << 0); // All but LSB set

            ulong actual = sut.RemovePiece(fullBoard, squareToRemove);

            Assert.Equal(expectedResult, actual);
        }

        [Fact(DisplayName = "Behavior check: Invalid positive index (64) currently results in no change")]
        public void RemovePiece_InvalidIndex64_ReturnsOriginalBoard()
        {
            // 1ul << 64 results in 0 because the 64th bit shifts off the end of the ulong.
            // Therefore, ~(0ul) is ulong.MaxValue, and ANDing with ulong.MaxValue results in no change.
            ulong initialPosition = 0b111ul;

            // Assert that the underlying runtime exception is thrown if validation isn't added
            Assert.Throws<System.ArgumentOutOfRangeException>(() => sut.RemovePiece(initialPosition, 64));
        }

        [Fact(DisplayName = "Behavior check: Negative index currently results in an exception")]
        public void RemovePiece_NegativeIndex_ThrowsException()
        {
            // C# throws a runtime exception for negative shift counts.
            ulong initialPosition = 0b1ul;

            // Assert that the underlying runtime exception is thrown if validation isn't added
            Assert.Throws<System.ArgumentOutOfRangeException>(() => sut.RemovePiece(initialPosition, -1));
        }

        [Theory(DisplayName = "Should add a piece (turn on a bit) at a specific square from an empty board")]
        // Test adding a piece to various positions on an empty board
        [InlineData(0, 0b1ul)]                          // Add A1 piece (LSB)
        [InlineData(1, 0b10ul)]                         // Add B1 piece
        [InlineData(7, 0b10000000ul)]                  // Add H1 piece
        [InlineData(31, 0b10000000000000000000000000000000ul)] // Add D4 piece (mid-board)
        [InlineData(63, 0b1000000000000000000000000000000000000000000000000000000000000000ul)] // Add H8 piece (MSB)
        public void AddPiece_ToEmptyBoard_ResultsInOnlyThatPiece(int squareToAdd, ulong expectedResult)
        {
            ulong emptyBoard = 0b0ul;

            // Act
            ulong actual = sut.AddPiece(emptyBoard, squareToAdd);

            // Assert
            Assert.Equal(expectedResult, actual);
        }

        [Fact(DisplayName = "Should add the target piece while leaving other existing pieces intact")]
        public void AddPiece_ToPartiallyOccupiedBoard_CombinesPositions()
        {
            // Board state with pieces at A1 (0) and D1 (3)
            ulong initialPosition = 0b00001001ul;

            // Expected board state after adding the piece at A2 (square 8)
            // Expected result should have bits 0, 3, and 8 set.
            ulong expectedResult = 0b00000001_00001001ul;

            // Act: Add a piece at A2 (square 8)
            ulong actual = sut.AddPiece(initialPosition, 8);

            // Assert
            Assert.Equal(expectedResult, actual);
        }

        [Fact(DisplayName = "Adding a piece to a square that is already occupied should result in no change")]
        public void AddPiece_ToOccupiedSquare_BoardStateRemainsSame()
        {
            // Board state with a piece at A1 (0)
            ulong initialPosition = 0b1ul;
            int squareToAdd = 0;

            // Act: Add a piece to A1 again
            ulong actual = sut.AddPiece(initialPosition, squareToAdd);

            // Assert: The board state should not change
            Assert.Equal(initialPosition, actual);
        }

        [Fact(DisplayName = "Adding a piece to a full board should result in a full board")]
        public void AddPiece_ToFullBoard_RemainsFull()
        {
            ulong fullBoard = ulong.MaxValue; // All 64 bits set to 1
            int squareToAdd = 32; // Any valid square

            ulong actual = sut.AddPiece(fullBoard, squareToAdd);

            Assert.Equal(fullBoard, actual);
        }

        // --- Validation/Exception Handling Tests ---
        // These tests verify your new validation logic works as expected.

        [Theory(DisplayName = "Should throw ArgumentOutOfRangeException for invalid positive square indices")]
        [InlineData(64)]
        [InlineData(65)]
        [InlineData(1000)]
        public void AddPiece_InvalidPositiveSquareIndex_ThrowsException(int invalidSquareIndex)
        {
            ulong anyBoard = 0b0ul;

            // Use Assert.Throws to verify the correct exception type is thrown
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                sut.AddPiece(anyBoard, invalidSquareIndex)
            );

            // Optional: Verify the exception message contains the invalid value
            Assert.Contains(invalidSquareIndex.ToString(), exception.Message);
        }

        [Theory(DisplayName = "Should throw ArgumentOutOfRangeException for invalid negative square indices")]
        [InlineData(-1)]
        [InlineData(-10)]
        public void AddPiece_InvalidNegativeSquareIndex_ThrowsException(int invalidSquareIndex)
        {
            ulong anyBoard = 0b0ul;

            // Note: C# bit shifting *would* throw a different exception (System.ArgumentOutOfRangeException
            // regarding the shift count itself) without your explicit `if` check. 
            // We test that your specific domain validation is hit first.
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                sut.AddPiece(anyBoard, invalidSquareIndex)
            );

            Assert.Contains(invalidSquareIndex.ToString(), exception.Message);
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
