using Interfaces.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Repositories;
using Services;

namespace ControllerTests
{
    public class AnalyserControllerTests
    {
        [Fact]
        public void GetDefaultPgnPath_ReturnsConfiguredDefaultFromOptions()
        {
            var chessRepoMock = new Mock<IChessRepository>();
            var etlServiceMock = new Mock<IEtlService>();
            var progressStoreMock = new Mock<IEtlProgressStore>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var pgnOptions = Options.Create(new Analyser.PgnOptions { DefaultFilePath = "C:\\Library\\PGN" });

            var controller = new Analyser.Controllers.Analyser(
                chessRepoMock.Object,
                etlServiceMock.Object,
                progressStoreMock.Object,
                scopeFactoryMock.Object,
                pgnOptions);
            var result = controller.GetDefaultPgnPath();

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = ok.Value;
            var prop = value?.GetType().GetProperty("defaultFilePath");
            Assert.NotNull(prop);
            Assert.Equal("C:\\Library\\PGN", prop.GetValue(value));
        }

        [Fact]
        public void LoadGames_Returns202AcceptedAndStartsEtlInBackground()
        {
            var chessRepoMock = new Mock<IChessRepository>();
            var etlServiceMock = new Mock<IEtlService>();
            etlServiceMock.Setup(e => e.LoadGamesToDatabase(It.IsAny<string>(), It.IsAny<IProgress<EtlProgress>>())).Returns(Task.CompletedTask);
            var progressStoreMock = new Mock<IEtlProgressStore>();
            var scopeMock = new Mock<IServiceScope>();
            var providerMock = new Mock<IServiceProvider>();
            providerMock.Setup(p => p.GetService(typeof(IEtlService))).Returns(etlServiceMock.Object);
            scopeMock.Setup(s => s.ServiceProvider).Returns(providerMock.Object);
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);
            var pgnOptions = Options.Create(new Analyser.PgnOptions { DefaultFilePath = "C:\\Library\\PGN" });

            var controller = new Analyser.Controllers.Analyser(
                chessRepoMock.Object,
                etlServiceMock.Object,
                progressStoreMock.Object,
                scopeFactoryMock.Object,
                pgnOptions);
            var loadGamesDto = new LoadGamesDto { FilePath = "C:\\Library\\PGN\\" };
            var result = controller.LoadGames(loadGamesDto);

            var accepted = Assert.IsType<AcceptedResult>(result);
            Assert.Equal(202, accepted.StatusCode);
        }

        [Fact]
        public void GetLoadGamesProgress_ReturnsProgressFromStore()
        {
            var chessRepoMock = new Mock<IChessRepository>();
            var etlServiceMock = new Mock<IEtlService>();
            var progressStoreMock = new Mock<IEtlProgressStore>();
            var progress = new EtlProgress { CurrentFileIndex = 0, TotalFiles = 5, Status = "Running" };
            progressStoreMock.Setup(s => s.Get()).Returns(progress);
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var pgnOptions = Options.Create(new Analyser.PgnOptions { DefaultFilePath = "C:\\Library\\PGN" });

            var controller = new Analyser.Controllers.Analyser(
                chessRepoMock.Object,
                etlServiceMock.Object,
                progressStoreMock.Object,
                scopeFactoryMock.Object,
                pgnOptions);
            var result = controller.GetLoadGamesProgress();

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<EtlProgress>(ok.Value);
            Assert.Equal("Running", value.Status);
            Assert.Equal(5, value.TotalFiles);
        }

        [Fact]
        public async Task GetGames_CallsRepositoryAndReturnsOkWithGames()
        {
            var games = new List<Game> { new Game { Name = "G", GameId = "1", Plies = new Dictionary<int, Ply>() } };
            var chessRepoMock = new Mock<IChessRepository>();
            chessRepoMock.Setup(r => r.GetGames()).ReturnsAsync(games);
            var etlServiceMock = new Mock<IEtlService>();
            var progressStoreMock = new Mock<IEtlProgressStore>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var pgnOptions = Options.Create(new Analyser.PgnOptions { DefaultFilePath = "C:\\Library\\PGN" });

            var controller = new Analyser.Controllers.Analyser(
                chessRepoMock.Object,
                etlServiceMock.Object,
                progressStoreMock.Object,
                scopeFactoryMock.Object,
                pgnOptions);
            var result = await controller.GetGames();

            chessRepoMock.Verify(r => r.GetGames(), Times.Once);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedGames = Assert.IsAssignableFrom<List<Game>>(okResult.Value);
            Assert.Single(returnedGames);
            Assert.Equal("G", returnedGames[0].Name);
        }
    }
}
