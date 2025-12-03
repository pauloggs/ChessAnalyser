using Interfaces;
using Interfaces.DTO;
using Moq;
using Services;
using Services.Helpers;
using static Interfaces.Constants;

namespace ServicesHelpersTests
{
    public class SourceSquareHelperTests
    {
        private Mock<IBitBoardManipulator> bitBoardManipulator;
        private readonly ISourceSquareHelper sut;

        public SourceSquareHelperTests()
        {
            bitBoardManipulator = new Mock<IBitBoardManipulator>();
            sut = new SourceSquareHelper(bitBoardManipulator.Object);
        }

        [Theory]
        [InlineData("Nbd2", -1, 1)] // 'b' -> 1
        [InlineData("R1e4", 0, -1)] // '1' -> 0
        [InlineData("Qh5", -1, -1)] // not specified
        [InlineData("e4", -1, -1)] // not specified
        [InlineData("exf4", -1, -1)] // not specified
        [InlineData("Nbxd2", -1, 1)] // 'b' -> 1
        [InlineData("Nbxd8=Q", -1, 1)] // 'b' -> 1
        [InlineData("Nxd8=Q", -1, -1)] // not specified
        [InlineData("Re4", -1, -1)] // not specified                                    
        [InlineData("R7xe7", 6, -1)] // Test cases for rank disambiguation with captures (Rook at rank 7 captures on e7)
        [InlineData("Bfxd4", -1, 5)] // 'f' -> 5
        [InlineData("Kh8", -1, -1)] // not specified    
        [InlineData("Nb1d2", 0, 1)] // 'b' -> 1, '1' -> 0, rare 'three knight' case
        [InlineData("Nf3xd4", 2, 5)] // 'f' -> 5, '3' -> 2, rare 'three knight' case
        [InlineData("", -1, -1)] // not specified
        [InlineData("e", -1, -1)] // not specified
        public void GetSourceRankAndOrFile_ShouldReturnExpectedResults(string rawMove, int expectedRank, int expectedFile)
        {
            // Act
            (int rank, int file) = sut.GetSourceRankAndOrFile(rawMove);

            // Assert
            Assert.Equal(expectedRank, rank);
            Assert.Equal(expectedFile, file);
        }

        [Fact]
        public void GetSourceSquare_ShouldReturnMoveNotFound_WhenPieceNotAtPotentialSource()
        {
            // Arrange
            var previousBoardPosition = new BoardPosition();
            int potentialSourceRank = 0;
            int potentialSourceFile = 0;
            char piece = 'N';
            Colour colour = Colour.W;
            bitBoardManipulator
                .Setup(m => m.ReadSquare(previousBoardPosition, piece, colour, potentialSourceRank, potentialSourceFile))
                .Returns(false);

            // Act
            int result = sut.GetSourceSquare(previousBoardPosition, potentialSourceRank, potentialSourceFile, piece, colour);

            // Assert
            Assert.Equal(Constants.MoveNotFound, result);
        }

        [Fact]
        public void GetSourceSquare_ShouldReturnSquare_WhenPieceAtPotentialSource()
        {
            // Arrange
            var previousBoardPosition = new BoardPosition();
            int potentialSourceRank = 0;
            int potentialSourceFile = 0;
            char piece = 'N';
            Colour colour = Colour.W;
            bitBoardManipulator
                .Setup(m => m.ReadSquare(previousBoardPosition, piece, colour, potentialSourceRank, potentialSourceFile))
                .Returns(true);

            // Act
            int result = sut.GetSourceSquare(previousBoardPosition, potentialSourceRank, potentialSourceFile, piece, colour);

            // Assert
            int expectedSquare = SquareHelper.GetSquareFromRankAndFile(potentialSourceRank, potentialSourceFile);
            Assert.Equal(expectedSquare, result);
        }

        [Fact]
        public void GetSourceSquare_ShouldReturnMoveNotFound_WhenPotentialSourceOutOfBounds()
        {
            // Arrange
            var previousBoardPosition = new BoardPosition();
            int potentialSourceRank = 8; // Invalid rank
            int potentialSourceFile = 0;
            char piece = 'N';
            Colour colour = Colour.W;

            // Act
            int result = sut.GetSourceSquare(previousBoardPosition, potentialSourceRank, potentialSourceFile, piece, colour);

            // Assert
            Assert.Equal(Constants.MoveNotFound, result);
        }

        [Fact]
        public void GetSourceSquare_ShouldReturnMoveNotFound_WhenPotentialSourceFileOutOfBounds()
        {
            // Arrange
            var previousBoardPosition = new BoardPosition();
            int potentialSourceRank = 0;
            int potentialSourceFile = -1; // Invalid file
            char piece = 'N';
            Colour colour = Colour.W;

            // Act
            int result = sut.GetSourceSquare(previousBoardPosition, potentialSourceRank, potentialSourceFile, piece, colour);

            // Assert
            Assert.Equal(Constants.MoveNotFound, result);
        }
    }
}
