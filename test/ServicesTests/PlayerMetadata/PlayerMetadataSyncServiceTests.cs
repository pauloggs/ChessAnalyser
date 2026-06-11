using Interfaces.DTO;
using Moq;
using Repositories;
using Services;

namespace ServicesTests.PlayerMetadata;

public class PlayerMetadataSyncServiceTests
{
    [Fact]
    public async Task SyncWorldChampionFlagsAsync_UpdatesOnlyChangedRows()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetPlayers()).ReturnsAsync(new List<Player>
        {
            new() { Id = 1, Surname = "Kasparov", Forenames = "Garry", WasWorldChampion = false },
            new() { Id = 2, Surname = "Tal", Forenames = "Mikhail", WasWorldChampion = true },
            new() { Id = 3, Surname = "Morphy", Forenames = "Paul", WasWorldChampion = false }
        });

        var sut = new PlayerMetadataSyncService(repo.Object);
        var result = await sut.SyncWorldChampionFlagsAsync();

        Assert.Equal(3, result.PlayersChecked);
        Assert.Equal(1, result.PlayersUpdated);
        repo.Verify(r => r.UpdatePlayerWasWorldChampionAsync(1, true, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.UpdatePlayerWasWorldChampionAsync(2, It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(r => r.UpdatePlayerWasWorldChampionAsync(3, It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
