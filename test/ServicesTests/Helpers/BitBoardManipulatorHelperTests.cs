using Services.Helpers;

namespace ServicesTests.Helpers
{    
    public class BitBoardManipulatorHelperTests
    {
        private readonly BitBoardManipulatorHelper sut;

        public BitBoardManipulatorHelperTests()
        {
            sut = new BitBoardManipulatorHelper();
        }

        [Theory(DisplayName = "Should return True when the piece exists at the specified file (0-7)")]
        [InlineData(0b00000001, 0, true)]  // File A (LSB)
        [InlineData(0b00000010, 1, true)]  // File B
        [InlineData(0b00000100, 2, true)]  // File C
        [InlineData(0b00001000, 3, true)]  // File D
        [InlineData(0b00010000, 4, true)]  // File E
        [InlineData(0b00100000, 5, true)]  // File F
        [InlineData(0b01000000, 6, true)]  // File G
        [InlineData(0b10000000, 7, true)]  // File H (MSB)
        public void IsPiecePresentAtFileInRank_WhenPieceExists_ReturnsTrue(byte ranks, int file, bool expected)
        {
            // Act
            bool actual = sut.IsPiecePresentAtFileInRank(ranks, file);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory(DisplayName = "Should return False when no piece exists at the specified file")]
        [InlineData(0b00000001, 1, false)] // Piece at 0, checking file 1
        [InlineData(0b10000000, 6, false)] // Piece at 7, checking file 6
        [InlineData(0b00010000, 0, false)] // Piece at 4, checking file 0
        [InlineData(0b00000000, 4, false)] // Empty rank, checking file 4
        public void IsPiecePresentAtFileInRank_WhenPieceDoesNotExist_ReturnsFalse(byte ranks, int file, bool expected)
        {
            // Act
            bool actual = sut.IsPiecePresentAtFileInRank(ranks, file);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsPiecePresentAtFileInRank_WhenMultiplePiecesExist_ReturnsCorrectStatus()
        {
            // Rank byte representing pieces on files A (0), D (3), and H (7)
            byte multiplePieces = 0b10001001;

            // Assert True for occupied files
            Assert.True(sut.IsPiecePresentAtFileInRank(multiplePieces, 0));
            Assert.True(sut.IsPiecePresentAtFileInRank(multiplePieces, 3));
            Assert.True(sut.IsPiecePresentAtFileInRank(multiplePieces, 7));

            // Assert False for empty files
            Assert.False(sut.IsPiecePresentAtFileInRank(multiplePieces, 1));
            Assert.False(sut.IsPiecePresentAtFileInRank(multiplePieces, 2));
        }

        [Fact]
        public void IsPiecePresentAtFileInRank_EmptyRank_ReturnsFalseForAllFiles()
        {
            byte emptyRank = 0b00000000;
            for (int file = 0; file < 8; file++)
            {
                Assert.False(sut.IsPiecePresentAtFileInRank(emptyRank, file), $"Should be false for file {file}");
            }
        }

        [Fact]
        public void IsPiecePresentAtFileInRank_FullRank_ReturnsTrueForAllFiles()
        {
            byte fullRank = 0b11111111; // All 8 files occupied
            for (int file = 0; file < 8; file++)
            {
                Assert.True(sut.IsPiecePresentAtFileInRank(fullRank, file), $"Should be true for file {file}");
            }
        }

        [Theory(DisplayName = "Should throw ArgumentOutOfRangeException for invalid positive file indices")]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(100)]
        public void IsPiecePresentAtFileInRank_InvalidPositiveFileIndex_ThrowsException(int invalidFileIndex)
        {
            byte anyRank = 0b00000001; // Data doesn't matter for this test

            // Use Assert.Throws to verify the correct exception type is thrown
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                sut.IsPiecePresentAtFileInRank(anyRank, invalidFileIndex)
            );

            // Optional: Verify the exception message contains the invalid value
            Assert.Contains(invalidFileIndex.ToString(), exception.Message);
        }

        [Theory(DisplayName = "Should throw ArgumentOutOfRangeException for invalid negative file indices")]
        [InlineData(-1)]
        [InlineData(-5)]
        public void IsPiecePresentAtFileInRank_InvalidNegativeFileIndex_ThrowsException(int invalidFileIndex)
        {
            byte anyRank = 0b00000001;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sut.IsPiecePresentAtFileInRank(anyRank, invalidFileIndex)
            );
        }
    }
}
