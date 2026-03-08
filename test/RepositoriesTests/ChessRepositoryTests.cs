using Interfaces.DTO;
using Microsoft.Extensions.Configuration;
using Moq;
using Repositories;

namespace RepositoriesTests
{
    public class ChessRepositoryTests
    {
        [Fact]
        public void Constructor_WithConfiguration_DoesNotThrow()
        {
            var connSection = new Mock<IConfigurationSection>();
            connSection.Setup(s => s["ChessConnection"]).Returns("Server=.;Database=Chess;Integrated Security=true;TrustServerCertificate=true");
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c.GetSection("ConnectionStrings")).Returns(connSection.Object);

            var _ = new ChessRepository(configMock.Object);
        }

        [Fact]
        public async Task GetProcessedGameIds_WhenConnectionFails_Throws()
        {
            var connSection = new Mock<IConfigurationSection>();
            connSection.Setup(s => s["ChessConnection"]).Returns("Server=InvalidServerThatDoesNotExist;Database=Chess;Connection Timeout=1");
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c.GetSection("ConnectionStrings")).Returns(connSection.Object);

            var sut = new ChessRepository(configMock.Object);

            await Assert.ThrowsAnyAsync<Exception>(() => sut.GetProcessedGameIds());
        }
    }
}
