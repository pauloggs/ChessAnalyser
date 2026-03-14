using Interfaces.DTO;
using Moq;
using Services;
using Services.Helpers;
using static Interfaces.Constants;

namespace ServicesTests
{
    public class BoardPositionServiceTests
    {
        [Fact]
        public void SetBoardPositions_WhenSingleGame_CallsGetStartingBoardPositionAndGetBoardPositionForPly()
        {
            var helperMock = new Mock<IBoardPositionsHelper>();
            var startingPosition = new BoardPosition();
            helperMock.Setup(h => h.GetStartingBoardPosition()).Returns(startingPosition);
            helperMock.Setup(h => h.SetWinner(It.IsAny<Game>(), It.IsAny<int>())).Returns(false);
            helperMock.Setup(h => h.GetBoardPositionForPly(It.IsAny<Game>(), It.IsAny<int>()))
                .Returns((Game g, int i) => new BoardPosition());

            var sut = new BoardPositionService(helperMock.Object);
            var game = new Game
            {
                Name = "Test",
                Plies = new Dictionary<int, Ply>
                {
                    { 0, new Ply { RawMove = "e4", Colour = Colour.W } },
                    { 1, new Ply { RawMove = "e5", Colour = Colour.B } }
                },
                BoardPositions = new Dictionary<int, BoardPosition>()
            };

            sut.SetBoardPositions(new List<Game> { game });

            helperMock.Verify(h => h.GetStartingBoardPosition(), Times.Once);
            helperMock.Verify(h => h.GetBoardPositionForPly(It.IsAny<Game>(), 0), Times.Once);
            helperMock.Verify(h => h.GetBoardPositionForPly(It.IsAny<Game>(), 1), Times.Once);
            helperMock.Verify(h => h.SetWinner(It.IsAny<Game>(), It.IsAny<int>()), Times.AtLeast(2));
            Assert.NotNull(game.InitialBoardPosition);
            Assert.Equal(3, game.BoardPositions.Count); // -1 (initial), 0, 1
            Assert.True(game.BoardPositions.ContainsKey(-1));
            Assert.True(game.BoardPositions.ContainsKey(0));
            Assert.True(game.BoardPositions.ContainsKey(1));
        }

        [Fact]
        public void SetBoardPositions_WhenSetWinnerReturnsTrue_StopsProcessingFurtherPlies()
        {
            var helperMock = new Mock<IBoardPositionsHelper>();
            helperMock.Setup(h => h.GetStartingBoardPosition()).Returns(new BoardPosition());
            helperMock.Setup(h => h.SetWinner(It.IsAny<Game>(), 0)).Returns(true);
            var sut = new BoardPositionService(helperMock.Object);
            var game = new Game
            {
                Name = "Test",
                Plies = new Dictionary<int, Ply>
                {
                    { 0, new Ply { RawMove = "1-0", Colour = Colour.W } },
                    { 1, new Ply { RawMove = "e5", Colour = Colour.B } }
                },
                BoardPositions = new Dictionary<int, BoardPosition>()
            };

            sut.SetBoardPositions(new List<Game> { game });

            helperMock.Verify(h => h.SetWinner(game, 0), Times.Once);
            helperMock.Verify(h => h.GetBoardPositionForPly(It.IsAny<Game>(), It.IsAny<int>()), Times.Never);
            Assert.Single(game.BoardPositions);
            Assert.True(game.BoardPositions.ContainsKey(-1));
        }
    }
}
