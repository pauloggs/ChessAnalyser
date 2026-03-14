using Interfaces.DTO;
using Moq;
using Services;

namespace ServicesTests
{
    public class EtlServiceTests
    {
        [Fact]
        public async Task LoadGamesToDatabase_WhenFilesAndGamesReturned_ProcessesOneGameAtATimeAndPersists()
        {
            var fileHandlerMock = new Mock<IFileHandler>();
            var pgnParserMock = new Mock<IPgnParser>();
            var persistenceMock = new Mock<IPersistenceService>();
            var boardPositionServiceMock = new Mock<IBoardPositionService>();
            var progressStoreMock = new Mock<IEtlProgressStore>();
            progressStoreMock.Setup(s => s.IsCancellationRequested).Returns(false);
            var playerResolverMock = new Mock<IPlayerResolver>();
            playerResolverMock.Setup(pr => pr.LoadKnownPlayersAsync()).Returns(Task.CompletedTask);
            playerResolverMock.Setup(pr => pr.ResolveGamePlayersAsync(It.IsAny<Game>()))
                .Callback<Game>(g => { g.WhitePlayerId = 1; g.BlackPlayerId = 2; })
                .Returns(Task.CompletedTask);

            var pgnFiles = new List<PgnFile> { new PgnFile { Name = "a.pgn", Contents = "[Event \"E\"]\n1. e4" } };
            var game = new Game { Name = "G", Plies = new Dictionary<int, Ply>(), GameId = "x" };

            fileHandlerMock.Setup(f => f.LoadPgnFiles(It.IsAny<string>())).Returns(pgnFiles);
            pgnParserMock.Setup(p => p.GetGameCountInFile(It.IsAny<PgnFile>())).Returns(1);
            pgnParserMock.Setup(p => p.EnumerateGamesFromPgnFile(It.IsAny<PgnFile>()))
                .Returns(new[] { game });
            persistenceMock.Setup(ps => ps.GetProcessedGameIds()).ReturnsAsync(new List<string>());

            var sut = new EtlService(
                fileHandlerMock.Object,
                pgnParserMock.Object,
                persistenceMock.Object,
                boardPositionServiceMock.Object,
                progressStoreMock.Object,
                playerResolverMock.Object);

            await sut.LoadGamesToDatabase("C:\\PGN");

            fileHandlerMock.Verify(f => f.LoadPgnFiles("C:\\PGN"), Times.Once);
            persistenceMock.Verify(ps => ps.GetProcessedGameIds(), Times.Exactly(2)); // Once for count pass, once before process pass
            boardPositionServiceMock.Verify(b => b.SetBoardPositions(It.Is<List<Game>>(l => l.Count == 1 && l[0].GameId == "x"), It.IsAny<List<GameParseError>>()), Times.Once);
            persistenceMock.Verify(ps => ps.InsertGames(It.Is<List<Game>>(l => l.Count == 1)), Times.Once);
            persistenceMock.Verify(ps => ps.InsertParseErrors(It.IsAny<List<GameParseError>>()), Times.Once);
        }

        [Fact]
        public async Task LoadGamesToDatabase_WhenGameAlreadyProcessed_SkipsSetBoardPositionsAndInsert()
        {
            var fileHandlerMock = new Mock<IFileHandler>();
            var pgnParserMock = new Mock<IPgnParser>();
            var persistenceMock = new Mock<IPersistenceService>();
            var boardPositionServiceMock = new Mock<IBoardPositionService>();
            var progressStoreMock = new Mock<IEtlProgressStore>();
            progressStoreMock.Setup(s => s.IsCancellationRequested).Returns(false);
            var playerResolverMock = new Mock<IPlayerResolver>();
            playerResolverMock.Setup(pr => pr.LoadKnownPlayersAsync()).Returns(Task.CompletedTask);
            playerResolverMock.Setup(pr => pr.ResolveGamePlayersAsync(It.IsAny<Game>()))
                .Callback<Game>(g => { g.WhitePlayerId = 1; g.BlackPlayerId = 2; })
                .Returns(Task.CompletedTask);

            var pgnFiles = new List<PgnFile> { new PgnFile { Name = "a.pgn", Contents = "[Event \"E\"]\n1. e4" } };
            var game = new Game { Name = "G", GameId = "x", Plies = new Dictionary<int, Ply>() };

            fileHandlerMock.Setup(f => f.LoadPgnFiles(It.IsAny<string>())).Returns(pgnFiles);
            pgnParserMock.Setup(p => p.GetGameCountInFile(It.IsAny<PgnFile>())).Returns(1);
            pgnParserMock.Setup(p => p.EnumerateGamesFromPgnFile(It.IsAny<PgnFile>())).Returns(new[] { game });
            persistenceMock.Setup(ps => ps.GetProcessedGameIds()).ReturnsAsync(new List<string> { "x" });

            var sut = new EtlService(
                fileHandlerMock.Object,
                pgnParserMock.Object,
                persistenceMock.Object,
                boardPositionServiceMock.Object,
                progressStoreMock.Object,
                playerResolverMock.Object);

            await sut.LoadGamesToDatabase("C:\\PGN");

            boardPositionServiceMock.Verify(b => b.SetBoardPositions(It.IsAny<List<Game>>(), It.IsAny<List<GameParseError>>()), Times.Never);
            persistenceMock.Verify(ps => ps.InsertGames(It.IsAny<List<Game>>()), Times.Never);
        }
    }
}
