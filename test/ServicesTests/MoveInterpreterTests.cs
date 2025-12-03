using Interfaces.DTO;
using Moq;
using Services;
using Services.Helpers;
using static Interfaces.Constants;

namespace ServicesTests
{
    public class MoveInterpreterTests
    {
        private readonly Mock<IMoveInterpreterHelper> moveInterpreterHelperMock;
        private readonly MoveInterpreter sut; // The class containing GetSourceSquare

        public MoveInterpreterTests()
        {
            moveInterpreterHelperMock = new Mock<IMoveInterpreterHelper>();
            sut = new MoveInterpreter(moveInterpreterHelperMock.Object);
        }

        [Fact]
        public void GetSourceAndDestinationSquares_ShouldThrowArgumentNullException_WhenPlyIsNull()
        {
            // Arrange
            BoardPosition previousBoardPosition = new BoardPosition();

            Ply ply = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.GetSourceAndDestinationSquares(previousBoardPosition, ply));
        }

        [Fact]
        public void GetSourceAndDestinationSquares_ShouldThrowException_WhenRawMoveIsInvalid()
        {
            // Arrange
            BoardPosition previousBoardPosition = new BoardPosition();
            Ply ply = new Ply { RawMove = "e" }; // Invalid move (less than 2 characters)
            Colour colour = Colour.W;

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => sut.GetSourceAndDestinationSquares(previousBoardPosition, ply));
            Assert.Contains("invalid move", exception.Message);
        }

        [Fact]
        public void GetSourceAndDestinationSquares_ShouldReturnExpectedValues()
        {
            // Arrange
            BoardPosition previousBoardPosition = new BoardPosition();
            Ply ply = new Ply { RawMove = "Nf3" };
            Colour colour = Colour.W;
            ply.Colour = colour;
            var expectedPiece = new Piece('N', 3.0);
            int expectedSourceSquare = 57; // Example source square
            int expectedDestinationSquare = 21; // Example destination square
            moveInterpreterHelperMock.Setup(m => m.RemoveCheck(ply)).Verifiable();
            moveInterpreterHelperMock.Setup(m => m.GetPiece(ply)).Returns(expectedPiece);
            moveInterpreterHelperMock.Setup(m => m.GetDestinationSquare(ply)).Returns(expectedDestinationSquare);
            moveInterpreterHelperMock.Setup(m => m.GetSourceSquare(previousBoardPosition, ply)).Returns(expectedSourceSquare);

            // Act
            var (piece, sourceSquare, destinationSquare) = sut.GetSourceAndDestinationSquares(previousBoardPosition, ply);

            // Assert
            Assert.Equal(expectedPiece, piece);
            Assert.Equal(expectedSourceSquare, sourceSquare);
            Assert.Equal(expectedDestinationSquare, destinationSquare);
            moveInterpreterHelperMock.Verify(m => m.RemoveCheck(ply), Times.Once);
            moveInterpreterHelperMock.Verify(m => m.GetPiece(ply), Times.Once);
            moveInterpreterHelperMock.Verify(m => m.GetDestinationSquare(ply), Times.Once);
            moveInterpreterHelperMock.Verify(m => m.GetSourceSquare(previousBoardPosition, ply), Times.Once);
        }

        [Fact]
        public void GetSourceAndDestinationSquares_ShouldHandleCheckRemovalCorrectly()
        {
            // Arrange
            BoardPosition previousBoardPosition = new BoardPosition();
            Ply ply = new Ply { RawMove = "e4+" }; // Move with check indicator
            Colour colour = Colour.W;
            ply.Colour = colour;    
            var expectedPiece = new Piece('P', 1.0);
            int expectedSourceSquare = 52; // Example source square
            int expectedDestinationSquare = 36; // Example destination square
            moveInterpreterHelperMock.Setup(m => m.RemoveCheck(ply)).Verifiable();
            moveInterpreterHelperMock.Setup(m => m.GetPiece(ply)).Returns(expectedPiece);
            moveInterpreterHelperMock.Setup(m => m.GetDestinationSquare(ply)).Returns(expectedDestinationSquare);
            moveInterpreterHelperMock.Setup(m => m.GetSourceSquare(previousBoardPosition, ply)).Returns(expectedSourceSquare);

            // Act
            var (piece, sourceSquare, destinationSquare) = sut.GetSourceAndDestinationSquares(previousBoardPosition, ply);

            // Assert
            Assert.Equal(expectedPiece, piece);
            Assert.Equal(expectedSourceSquare, sourceSquare);
            Assert.Equal(expectedDestinationSquare, destinationSquare);
            moveInterpreterHelperMock.Verify(m => m.RemoveCheck(ply), Times.Once);
        }
    }
}
