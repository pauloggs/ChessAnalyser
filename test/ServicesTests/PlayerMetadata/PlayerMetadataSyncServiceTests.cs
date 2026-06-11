using Interfaces.DTO;
using Moq;
using Repositories;
using Services;
using Services.PlayerMetadata;

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

        var matcher = new Mock<IWorldChampionMatcher>();
        matcher.Setup(m => m.EnsureLoadedAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        matcher.Setup(m => m.IsWorldChampion("Kasparov", "Garry")).Returns(true);
        matcher.Setup(m => m.IsWorldChampion("Tal", "Mikhail")).Returns(true);
        matcher.Setup(m => m.IsWorldChampion("Morphy", "Paul")).Returns(false);

        var enricher = new Mock<IPlayerFideMetadataEnricher>();
        var sut = new PlayerMetadataSyncService(repo.Object, matcher.Object, enricher.Object);
        var result = await sut.SyncWorldChampionFlagsAsync();

        Assert.Equal(3, result.PlayersChecked);
        Assert.Equal(1, result.PlayersUpdated);
        repo.Verify(r => r.UpdatePlayerWasWorldChampionAsync(1, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BackfillFideMetadataAsync_DelegatesToEnricher()
    {
        var expected = new FideMetadataSyncResult { PlayersChecked = 10, PlayersUpdated = 3 };
        var enricher = new Mock<IPlayerFideMetadataEnricher>();
        enricher.Setup(e => e.BackfillAllAsync(false, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var sut = new PlayerMetadataSyncService(
            Mock.Of<IChessRepository>(),
            Mock.Of<IWorldChampionMatcher>(),
            enricher.Object);

        var result = await sut.BackfillFideMetadataAsync();

        Assert.Equal(10, result.PlayersChecked);
        Assert.Equal(3, result.PlayersUpdated);
    }
}
