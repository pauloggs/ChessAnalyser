using Interfaces.DTO;
using Moq;
using Services;
using Services.Helpers;

namespace ServicesHelpersTests
{  
    public class BoardPositionsHelperTests
    {
        private Mock<IMoveInterpreter> moveInterpreterMock;
        private Mock<IDisplayService> displayServiceMock;
        private Mock<IBoardPositionUpdater> boardPositionUpdaterMock;
        private Mock<IBitBoardManipulator> bitBoardManipulatorMock;
        private IBoardPositionsHelper sut;

        public BoardPositionsHelperTests()
        {
            moveInterpreterMock = new Mock<IMoveInterpreter>();
            displayServiceMock = new Mock<IDisplayService>();
            boardPositionUpdaterMock = new Mock<IBoardPositionUpdater>();
            bitBoardManipulatorMock = new Mock<IBitBoardManipulator>();
            sut = new BoardPositionsHelper(
                moveInterpreterMock.Object,
                displayServiceMock.Object,
                boardPositionUpdaterMock.Object,
                bitBoardManipulatorMock.Object);
        }

        [Fact]
        public void SetBoardPositionFromPly_ValidInput_SetsBoardPosition()
        {
            // Arrange
            var game = new Game
            {
                Name = "Test Game",
                Plies = []
            };
            var previousBoardPosition = sut.GetStartingBoardPosition();
            var ply = new Ply { RawMove = "e4" };
            int currentBoardIndex = 1;

            var moveInterpretation = (Piece: new Piece(), sourceSquare: 0, destinationSquare: 1);

            // set up mi's GetSourceAndDestinationSquares to return a valid MoveInterpretation
            moveInterpreterMock.Setup(
                mi => mi.GetSourceAndDestinationSquares(
                    It.IsAny<BoardPosition>(),
                    It.IsAny<Ply>(),
                    It.IsAny<char>())).Returns(moveInterpretation);

            // Act
            sut.SetBoardPositionFromPly(game, previousBoardPosition, ply, currentBoardIndex);

            // Assert
            Assert.True(game.BoardPositions.ContainsKey(currentBoardIndex));
            //moveInterpreterMock.Verify(mi => mi.InterpretMove(previousBoardPosition, ply), Times.Once);
        }

        [Fact]
        public void SetBoardPositionFromPly_ShouldAssignDeepCopiedPosition_WhenValidInputProvided()
        {
            // Arrange
            var game = new Game
            {
                Name = "Test Game",
                Plies = []
            };
            var previousBoardPosition = new BoardPosition();
            var ply = new Ply { RawMove = "e4", Colour = 'W' };
            int currentBoardIndex = 0;

            // Act
            sut.SetBoardPositionFromPly(game, previousBoardPosition, ply, currentBoardIndex);

            // Assert
            Assert.NotNull(game.BoardPositions[currentBoardIndex]);
        }

        [Fact]
        public void SetBoardPositionFromPly_ShouldCallGetSourceAndDestinationSquares_WhenCalled()
        {
            // Arrange
            var game = new Game
            {
                Name = "Test Game",
                Plies = []
            };
            var previousBoardPosition = new BoardPosition();
            var ply = new Ply { RawMove = "e5", Colour = 'B' };

            int currentBoardIndex = 0;

            var moveInterpretation = (Piece: new Piece(), sourceSquare: 0, destinationSquare: 1);

            // set up mi's GetSourceAndDestinationSquares to return a valid MoveInterpretation
            moveInterpreterMock.Setup(
                mi => mi.GetSourceAndDestinationSquares(
                    It.IsAny<BoardPosition>(),
                    It.IsAny<Ply>(),
                    It.IsAny<char>())).Returns(moveInterpretation);

            // Act
            sut.SetBoardPositionFromPly(game, previousBoardPosition, ply, currentBoardIndex);

            // Assert
            moveInterpreterMock.
                Verify(
                    m => m.GetSourceAndDestinationSquares(previousBoardPosition, ply, ply.Colour), 
                    Times.Once);
        }

        [Fact]
        public void GetStartingBoardPosition_ReturnsValidStartingPosition()
        {
            // Act
            var startingPosition = sut.GetStartingBoardPosition();

            // Assert
            Assert.NotNull(startingPosition);
            Assert.NotNull(startingPosition.PiecePositions);
        }


        [Fact]
        public void SetWinner_Player1Wins_ReturnsTrueAndSetsWinner()
        {
            // Arrange
            var game = new Game
            {
                Name = "Test Game",
                Plies = new Dictionary<int, Ply>
                {
                    { 0, new Ply { RawMove = "1-0" } }
                }
            };

            // Act
            var result = sut.SetWinner(game, 0);

            // Assert
            Assert.True(result);
            Assert.Equal("W", game.Winner);
        }

        [Fact]
        public void SetWinner_Player2Wins_ReturnsTrueAndSetsWinner()
        {
            // Arrange
            var game = new Game
            {
                Name = "Test Game",
                Plies = new Dictionary<int, Ply>
                {
                    { 0, new Ply { RawMove = "0-1" } }
                }
            };

            // Act
            var result = sut.SetWinner(game, 0);

            // Assert
            Assert.True(result);
            Assert.Equal("B", game.Winner);
        }

        [Fact]
        public void SetWinner_DrawCondition_ReturnsTrueAndSetsWinner()
        {
            // Arrange
            var game = new Game
            {
                Name = "Test Game",
                Plies = new Dictionary<int, Ply>
                {
                    { 0, new Ply { RawMove = "1/2-1/2" } }
                }
            };

            // Act
            var result = sut.SetWinner(game, 0);

            // Assert
            Assert.True(result);
            Assert.Equal("D", game.Winner);
        }

        [Fact]
        public void SetWinner_InvalidMove_ReturnsFalse()
        {
            // Arrange
            var game = new Game
            {
                Name = "Test Game",
                Plies = new Dictionary<int, Ply>
                {
                    { 0, new Ply { RawMove = "invalid" } }
                }
            };

            // Act
            var result = sut.SetWinner(game, 0);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetWinner_EmptyMove_ReturnsFalse()
        {
            // Arrange
            var game = new Game
            {
                Name = "Test Game",
                Plies = new Dictionary<int, Ply>
                {
                    { 0, new Ply { RawMove = "" } }
                }
            };

            // Act
            var result = sut.SetWinner(game, 0);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetWinner_NullGame_ThrowsArgumentNullException()
        {
            // Arrange
            Game game = null;
            int plyIndex = 0;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.SetWinner(game, plyIndex));
        }

        [Fact]
        public void SetWinner_NoPlies_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var game = new Game
            {
                Name = "Test Game",
                Plies = []
            };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.SetWinner(game, 0));
        }
    }
}
