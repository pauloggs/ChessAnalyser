using Interfaces.DTO;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Repositories;
using Services;

namespace ControllerTests
{
    public class AnalyserControllerTests
    {
        [Fact]
        public async Task LoadGames_CallsEtlServiceWithFilePathAndReturnsOk()
        {
            var chessRepoMock = new Mock<IChessRepository>();
            var etlServiceMock = new Mock<IEtlService>();
            etlServiceMock.Setup(e => e.LoadGamesToDatabase(It.IsAny<string>())).Returns(Task.CompletedTask);

            var controller = new Analyser.Controllers.Analyser(chessRepoMock.Object, etlServiceMock.Object);
            var result = await controller.LoadGames("C:\\PGN\\");

            etlServiceMock.Verify(e => e.LoadGamesToDatabase("C:\\PGN\\"), Times.Once);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task LoadGames_WhenEtlThrows_Returns500()
        {
            var chessRepoMock = new Mock<IChessRepository>();
            var etlServiceMock = new Mock<IEtlService>();
            etlServiceMock.Setup(e => e.LoadGamesToDatabase(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("ETL failed"));

            var controller = new Analyser.Controllers.Analyser(chessRepoMock.Object, etlServiceMock.Object);
            var result = await controller.LoadGames("C:\\PGN");

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetGames_CallsRepositoryAndReturnsOkWithGames()
        {
            var games = new List<Game> { new Game { Name = "G", GameId = "1", Plies = new Dictionary<int, Ply>() } };
            var chessRepoMock = new Mock<IChessRepository>();
            chessRepoMock.Setup(r => r.GetGames()).ReturnsAsync(games);
            var etlServiceMock = new Mock<IEtlService>();

            var controller = new Analyser.Controllers.Analyser(chessRepoMock.Object, etlServiceMock.Object);
            var result = await controller.GetGames();

            chessRepoMock.Verify(r => r.GetGames(), Times.Once);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedGames = Assert.IsAssignableFrom<List<Game>>(okResult.Value);
            Assert.Single(returnedGames);
            Assert.Equal("G", returnedGames[0].Name);
        }
    }
}
