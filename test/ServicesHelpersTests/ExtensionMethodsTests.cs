using Interfaces;
using Interfaces.DTO;

namespace ServicesHelpersTests
{
    public class ExtensionMethodsTests
    {
        [Fact]
        public void DeepCopy_WhenBoardPositionNotNull_ReturnsIndependentCopy()
        {
            var original = new BoardPosition();
            original.PiecePositions["WP"] = 0xFF00UL;
            original.EnPassantTargetFile = 'e';

            var copy = original.DeepCopy();

            Assert.NotNull(copy);
            Assert.NotSame(original, copy);
            Assert.Equal(original.PiecePositions["WP"], copy.PiecePositions["WP"]);
            Assert.Equal(original.EnPassantTargetFile, copy.EnPassantTargetFile);
            copy.PiecePositions["WP"] = 0UL;
            copy.EnPassantTargetFile = null;
            Assert.Equal(0xFF00UL, original.PiecePositions["WP"]);
            Assert.Equal('e', original.EnPassantTargetFile);
        }

        [Fact]
        public void DeepCopy_WhenBoardPositionNull_ThrowsArgumentNullException()
        {
            BoardPosition? nullBoard = null;
            Assert.Throws<ArgumentNullException>(() => nullBoard!.DeepCopy());
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(3, 4, 28)]
        [InlineData(7, 7, 63)]
        public void GetSquareFromRankAndFile_ReturnsCorrectSquare(int rank, int file, int expected)
        {
            var result = ExtensionMethods.GetSquareFromRankAndFile(rank, file);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(0, "a1")]
        [InlineData(28, "e4")]
        [InlineData(63, "h8")]
        public void Algebraic_ReturnsCorrectNotation(int square, string expected)
        {
            var result = square.Algebraic();
            Assert.Equal(expected, result);
        }
    }
}
