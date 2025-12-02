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
