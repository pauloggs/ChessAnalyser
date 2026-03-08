using Interfaces.DTO;
using Moq;
using Services;

namespace ServicesTests
{
    public class EtlServiceTests
    {
        [Fact]
        public async Task LoadGamesToDatabase_WhenFilesAndGamesReturned_CallsSetBoardPositionsAndInsertGames()
        {
            var fileHandlerMock = new Mock<IFileHandler>();
            var pgnParserMock = new Mock<IPgnParser>();
            var persistenceMock = new Mock<IPersistenceService>();
            var boardPositionServiceMock = new Mock<IBoardPositionService>();

            var pgnFiles = new List<PgnFile> { new PgnFile { Name = "a.pgn", Contents = "[Event \"E\"]\n1. e4" } };
            var games = new List<Game> { new Game { Name = "G", Plies = new Dictionary<int, Ply>(), GameId = "x" } };
            var unprocessed = new List<Game> { games[0] };

            fileHandlerMock.Setup(f => f.LoadPgnFiles(It.IsAny<string>())).Returns(pgnFiles);
            pgnParserMock.Setup(p => p.GetGamesFromPgnFiles(pgnFiles)).Returns(games);
            persistenceMock.Setup(ps => ps.GetUnprocessedGames(games)).ReturnsAsync(unprocessed);

            var sut = new EtlService(
                fileHandlerMock.Object,
                pgnParserMock.Object,
                persistenceMock.Object,
                boardPositionServiceMock.Object);

            await sut.LoadGamesToDatabase("C:\\PGN");

            fileHandlerMock.Verify(f => f.LoadPgnFiles("C:\\PGN"), Times.Once);
            pgnParserMock.Verify(p => p.GetGamesFromPgnFiles(pgnFiles), Times.Once);
            persistenceMock.Verify(ps => ps.GetUnprocessedGames(games), Times.Once);
            boardPositionServiceMock.Verify(b => b.SetBoardPositions(unprocessed), Times.Once);
            persistenceMock.Verify(ps => ps.InsertGames(unprocessed), Times.Once);
        }

        [Fact]
        public async Task LoadGamesToDatabase_WhenNoUnprocessedGames_DoesNotCallSetBoardPositionsOrInsertGames()
        {
            var fileHandlerMock = new Mock<IFileHandler>();
            var pgnParserMock = new Mock<IPgnParser>();
            var persistenceMock = new Mock<IPersistenceService>();
            var boardPositionServiceMock = new Mock<IBoardPositionService>();

            var pgnFiles = new List<PgnFile> { new PgnFile { Name = "a.pgn", Contents = "[Event \"E\"]\n1. e4" } };
            var games = new List<Game> { new Game { Name = "G", GameId = "x", Plies = new Dictionary<int, Ply>() } };

            fileHandlerMock.Setup(f => f.LoadPgnFiles(It.IsAny<string>())).Returns(pgnFiles);
            pgnParserMock.Setup(p => p.GetGamesFromPgnFiles(It.IsAny<List<PgnFile>>())).Returns(games);
            persistenceMock.Setup(ps => ps.GetUnprocessedGames(It.IsAny<List<Game>>())).ReturnsAsync(new List<Game>());

            var sut = new EtlService(
                fileHandlerMock.Object,
                pgnParserMock.Object,
                persistenceMock.Object,
                boardPositionServiceMock.Object);

            await sut.LoadGamesToDatabase("C:\\PGN");

            boardPositionServiceMock.Verify(b => b.SetBoardPositions(It.IsAny<List<Game>>()), Times.Never);
            persistenceMock.Verify(ps => ps.InsertGames(It.IsAny<List<Game>>()), Times.Never);
        }
    }
}
