using Interfaces.DTO;
using Moq;
using Services;
using Services.Helpers;
using static Interfaces.Constants;

namespace ServicesTests.Helpers
{
    public class PawnMoveInterpreterTests
    {
        [Fact]
        public void GetSourceSquare_OneSquareWhiteNonCaptureMove_ReturnsCorrectSourceSquare()
        {
            // Arrange
            var bitBoardManipulator = new Mock<IBitBoardManipulator>();
            var pawnMoveInterpreter = new PawnMoveInterpreter(bitBoardManipulator.Object);
            var previousBoardPosition = new BoardPosition();
            var ply = new Ply
            {
                Colour = Colour.W,
                Piece = Pieces['P'],
                DestinationRank = 4,
                DestinationFile = 4,
                IsCapture = false,
                RawMove = "e4"
            };
            // Mock the bitBoardManipulator to return true for the expected source square
            bitBoardManipulator.Setup(b => b.ReadSquare(
                previousBoardPosition,
                ply.Piece,
                Colour.W,
                3, // Rank 3 (one square behind rank 4)
                4  // File e
            )).Returns(true);

            // Act
            var sourceSquare = pawnMoveInterpreter.GetSourceSquare(previousBoardPosition, ply);

            // Assert
            Assert.Equal(SquareHelper.GetSquareFromRankAndFile(3, 4), sourceSquare);
        }

        [Fact]
        public void GetSourceSquare_TwoSquareBlackNonCaptureMove_ReturnsCorrectSourceSquare()
        {
            // Arrange
            var bitBoardManipulator = new Mock<IBitBoardManipulator>();
            var pawnMoveInterpreter = new PawnMoveInterpreter(bitBoardManipulator.Object);
            var previousBoardPosition = new BoardPosition();
            var ply = new Ply
            {
                Colour = Colour.B,
                Piece = Pieces['P'],
                DestinationRank = 5,
                DestinationFile = 3,
                IsCapture = false,
                RawMove = "d5"
            };
            // Mock the bitBoardManipulator to return true for the expected source square
            bitBoardManipulator.Setup(b => b.ReadSquare(
                previousBoardPosition,
                ply.Piece,
                Colour.B,
                7, // Rank 7 (two squares behind rank 5)
                3  // File d
            )).Returns(true);
            // Act
            var sourceSquare = pawnMoveInterpreter.GetSourceSquare(previousBoardPosition, ply);
            // Assert
            Assert.Equal(SquareHelper.GetSquareFromRankAndFile(7, 3), sourceSquare);
        }

        [Fact]
        public void GetSourceSquare_CaptureMove_ReturnsCorrectSourceSquare()
        {
            // Arrange
            var bitBoardManipulator = new Mock<IBitBoardManipulator>();
            var pawnMoveInterpreter = new PawnMoveInterpreter(bitBoardManipulator.Object);
            var previousBoardPosition = new BoardPosition();
            var ply = new Ply
            {
                Colour = Colour.W,
                Piece = Pieces['P'],
                DestinationRank = 5,
                DestinationFile = 4,
                IsCapture = true,
                RawMove = "exd5"
            };
            // Act
            var sourceSquare = pawnMoveInterpreter.GetSourceSquare(previousBoardPosition, ply);
            // Assert
            Assert.Equal(SquareHelper.GetSquareFromRankAndFile(4, 4), sourceSquare);
        }

        [Fact]
        public void GetSourceSquare_NoSourceSquareFound_ThrowsException()
        {
            // Arrange
            var bitBoardManipulator = new Mock<IBitBoardManipulator>();
            var pawnMoveInterpreter = new PawnMoveInterpreter(bitBoardManipulator.Object);
            var previousBoardPosition = new BoardPosition();
            var ply = new Ply
            {
                Colour = Colour.W,
                Piece = Pieces['P'],
                DestinationRank = 4,
                DestinationFile = 4,
                IsCapture = false,
                RawMove = "e4"
            };
            // Mock the bitBoardManipulator to return false for all potential source squares
            bitBoardManipulator.Setup(b => b.ReadSquare(
                It.IsAny<BoardPosition>(),
                It.IsAny<Piece>(),
                It.IsAny<Colour>(),
                It.IsAny<int>(),
                It.IsAny<int>()
            )).Returns(false);
            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                pawnMoveInterpreter.GetSourceSquare(previousBoardPosition, ply));
            Assert.Contains("no source square found", exception.Message);
        }

        [Fact]
        public void GetSourceSquare_CaptureMoveWithDifferentFile_ReturnsCorrectSourceSquare()
        {
            // Arrange
            var bitBoardManipulator = new Mock<IBitBoardManipulator>();
            var pawnMoveInterpreter = new PawnMoveInterpreter(bitBoardManipulator.Object);
            var previousBoardPosition = new BoardPosition();
            var ply = new Ply
            {
                Colour = Colour.B,
                Piece = Pieces['P'],
                DestinationRank = 4,
                DestinationFile = 3,
                IsCapture = true,
                RawMove = "dxe4"
            };
            // Act
            var sourceSquare = pawnMoveInterpreter.GetSourceSquare(previousBoardPosition, ply);
            // Assert
            Assert.Equal(SquareHelper.GetSquareFromRankAndFile(5, 3), sourceSquare);
        }
    }
}
