using Interfaces.DTO;
using Services.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ServicesHelpersTests
{
    public class SquareHelperTests
    {
        [Theory]
        [InlineData(-1, -1, true)]
        [InlineData(-1, 8, true)]
        [InlineData(8, 8, true)]
        [InlineData(8, -1, true)]
        [InlineData(0, 0, false)]
        [InlineData(0, 7, false)]
        [InlineData(7, 0, false)]
        [InlineData(7, 7, false)]
        public void GetSquareFromRankAndFile_ThrowException(int rank, int file, bool shouldThrowException)
        {
            // Act & Assert
            if (shouldThrowException)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => SquareHelper.GetSquareFromRankAndFile(rank, file));
            }
            else
            {
                var square = SquareHelper.GetSquareFromRankAndFile(rank, file);
                Assert.True(square >= 0);
            }
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(3, 0, 24)]
        [InlineData(3, 5, 29)]
        [InlineData(0, 5, 5)]
        public void GetSquareFromRankAndFile_ShouldReturnExpectedResults(int rank, int file, int expectedSquare)
        {
            // Act
            var actualSquare = SquareHelper.GetSquareFromRankAndFile(rank, file);

            // Assert
            Assert.Equal(expectedSquare, actualSquare);
        }
    }
}
