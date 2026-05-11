using Interfaces.DTO;
using Analyser.Models;
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

            var controller = new Analyser.Controllers.AnalyserController(
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

            var controller = new Analyser.Controllers.AnalyserController(
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

            var controller = new Analyser.Controllers.AnalyserController(
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
        public async Task GetPlayers_ReturnsOrderedPlayerOptionsWithDisplayNames()
        {
            var players = new List<Player>
            {
                new() { Id = 2, Surname = "Tal", Forenames = "Mikhail" },
                new() { Id = 1, Surname = "Botvinnik", Forenames = "Mikhail" },
                new() { Id = 3, Surname = "Capablanca", Forenames = "" }
            };
            var chessRepoMock = new Mock<IChessRepository>();
            chessRepoMock.Setup(r => r.GetPlayers()).ReturnsAsync(players);
            var etlServiceMock = new Mock<IEtlService>();
            var progressStoreMock = new Mock<IEtlProgressStore>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var pgnOptions = Options.Create(new Analyser.PgnOptions { DefaultFilePath = "C:\\Library\\PGN" });

            var controller = new Analyser.Controllers.AnalyserController(
                chessRepoMock.Object,
                etlServiceMock.Object,
                progressStoreMock.Object,
                scopeFactoryMock.Object,
                pgnOptions);

            var result = await controller.GetPlayers();

            var ok = Assert.IsType<OkObjectResult>(result);
            var options = Assert.IsType<List<PlayerOptionResponse>>(ok.Value);
            Assert.Equal([1, 3, 2], options.Select(o => o.Id).ToArray());
            Assert.Equal("Botvinnik, Mikhail", options[0].DisplayName);
            Assert.Equal("Capablanca", options[1].DisplayName);
            Assert.Equal("Tal, Mikhail", options[2].DisplayName);
        }

        [Fact]
        public async Task GetGames_CallsRepositoryAndReturnsOkWithPagedGames()
        {
            var games = new List<Game> { new Game { Name = "G", GameId = "1", Plies = new Dictionary<int, Ply>() } };
            var page = new PagedResult<Game>
            {
                Items = games,
                Page = 1,
                PageSize = 50,
                TotalCount = 1
            };
            var chessRepoMock = new Mock<IChessRepository>();
            chessRepoMock
                .Setup(r => r.GetGamesPage(1, 50, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
            var etlServiceMock = new Mock<IEtlService>();
            var progressStoreMock = new Mock<IEtlProgressStore>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var pgnOptions = Options.Create(new Analyser.PgnOptions { DefaultFilePath = "C:\\Library\\PGN" });

            var controller = new Analyser.Controllers.AnalyserController(
                chessRepoMock.Object,
                etlServiceMock.Object,
                progressStoreMock.Object,
                scopeFactoryMock.Object,
                pgnOptions);
            var result = await controller.GetGames();

            chessRepoMock.Verify(r => r.GetGamesPage(1, 50, null, It.IsAny<CancellationToken>()), Times.Once);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<PagedResult<Game>>(okResult.Value);
            Assert.Single(returned.Items);
            Assert.Equal("G", returned.Items[0].Name);
            Assert.Equal(1, returned.TotalCount);
        }

        [Fact]
        public async Task GetGames_WithFilters_PassesFiltersToRepository()
        {
            var page = new PagedResult<Game> { Items = [], Page = 1, PageSize = 10, TotalCount = 0 };
            var chessRepoMock = new Mock<IChessRepository>();
            chessRepoMock
                .Setup(r => r.GetGamesPage(
                    1,
                    10,
                    It.Is<GamePageFilters>(f =>
                        f != null
                        && f.MinGameYear == 1990
                        && f.MaxGameYear == 2000
                        && f.WhitePlayerId == 3
                        && f.BlackPlayerId == 7
                        && f.Eco == "B90"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
            var etlServiceMock = new Mock<IEtlService>();
            var progressStoreMock = new Mock<IEtlProgressStore>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var pgnOptions = Options.Create(new Analyser.PgnOptions { DefaultFilePath = "C:\\Library\\PGN" });

            var controller = new Analyser.Controllers.AnalyserController(
                chessRepoMock.Object,
                etlServiceMock.Object,
                progressStoreMock.Object,
                scopeFactoryMock.Object,
                pgnOptions);

            await controller.GetGames(1, 10, 1990, 2000, 3, 7, "B90");

            chessRepoMock.Verify(
                r => r.GetGamesPage(
                    1,
                    10,
                    It.Is<GamePageFilters>(f =>
                        f != null
                        && f.MinGameYear == 1990
                        && f.MaxGameYear == 2000
                        && f.WhitePlayerId == 3
                        && f.BlackPlayerId == 7
                        && f.Eco == "B90"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetGames_WhenMinYearGreaterThanMaxYear_ReturnsBadRequest()
        {
            var chessRepoMock = new Mock<IChessRepository>();
            var etlServiceMock = new Mock<IEtlService>();
            var progressStoreMock = new Mock<IEtlProgressStore>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var pgnOptions = Options.Create(new Analyser.PgnOptions { DefaultFilePath = "C:\\Library\\PGN" });

            var controller = new Analyser.Controllers.AnalyserController(
                chessRepoMock.Object,
                etlServiceMock.Object,
                progressStoreMock.Object,
                scopeFactoryMock.Object,
                pgnOptions);

            var result = await controller.GetGames(1, 50, 2010, 2000);

            Assert.IsType<BadRequestObjectResult>(result);
            chessRepoMock.Verify(
                r => r.GetGamesPage(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<GamePageFilters?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetGames_WhenEcoTooLong_ReturnsBadRequest()
        {
            var chessRepoMock = new Mock<IChessRepository>();
            var etlServiceMock = new Mock<IEtlService>();
            var progressStoreMock = new Mock<IEtlProgressStore>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var pgnOptions = Options.Create(new Analyser.PgnOptions { DefaultFilePath = "C:\\Library\\PGN" });

            var controller = new Analyser.Controllers.AnalyserController(
                chessRepoMock.Object,
                etlServiceMock.Object,
                progressStoreMock.Object,
                scopeFactoryMock.Object,
                pgnOptions);

            var result = await controller.GetGames(1, 50, eco: new string('A', 17));

            Assert.IsType<BadRequestObjectResult>(result);
            chessRepoMock.Verify(
                r => r.GetGamesPage(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<GamePageFilters?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
