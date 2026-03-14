using Interfaces.DTO;
using Moq;
using Repositories;
using Services;

namespace ServicesTests
{
    public class PersistenceServiceTests
    {
        [Fact]
        public async Task GetUnprocessedGames_WhenNoProcessedIds_ReturnsAllGames()
        {
            var repoMock = new Mock<IChessRepository>();
            repoMock.Setup(r => r.GetProcessedGameIds()).ReturnsAsync(new List<string>());
            var sut = new PersistenceService(repoMock.Object);
            var games = new List<Game>
            {
                new Game { GameId = "id1", Name = "G1", Plies = new Dictionary<int, Ply>() },
                new Game { GameId = "id2", Name = "G2", Plies = new Dictionary<int, Ply>() }
            };

            var result = await sut.GetUnprocessedGames(games);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetUnprocessedGames_WhenSomeProcessed_FiltersThemOut()
        {
            var repoMock = new Mock<IChessRepository>();
            repoMock.Setup(r => r.GetProcessedGameIds()).ReturnsAsync(new List<string> { "id1" });
            var sut = new PersistenceService(repoMock.Object);
            var games = new List<Game>
            {
                new Game { GameId = "id1", Name = "G1", Plies = new Dictionary<int, Ply>() },
                new Game { GameId = "id2", Name = "G2", Plies = new Dictionary<int, Ply>() }
            };

            var result = await sut.GetUnprocessedGames(games);

            Assert.Single(result);
            Assert.Equal("id2", result[0].GameId);
        }

        [Fact]
        public async Task InsertGames_CallsInsertGameAndInsertBoardPositionsForEachGame()
        {
            var repoMock = new Mock<IChessRepository>();
            repoMock.Setup(r => r.InsertGame(It.IsAny<Game>())).ReturnsAsync(42);
            repoMock.Setup(r => r.InsertBoardPositions(It.IsAny<Game>(), It.IsAny<int>())).Returns(Task.CompletedTask);
            var sut = new PersistenceService(repoMock.Object);
            var games = new List<Game>
            {
                new Game { GameId = "id1", Name = "G1", Plies = new Dictionary<int, Ply>(), BoardPositions = new Dictionary<int, BoardPosition>() },
                new Game { GameId = "id2", Name = "G2", Plies = new Dictionary<int, Ply>(), BoardPositions = new Dictionary<int, BoardPosition>() }
            };

            await sut.InsertGames(games);

            repoMock.Verify(r => r.InsertGame(It.IsAny<Game>()), Times.Exactly(2));
            repoMock.Verify(r => r.InsertBoardPositions(It.IsAny<Game>(), 42), Times.Exactly(2));
        }
    }
}
