using Interfaces.DTO;
using Interfaces.DTO.Ref;
using Moq;
using Repositories;
using Services.PlayerMetadata;

namespace ServicesTests.PlayerMetadata;

public class PlayerMetadataLinkingServiceTests
{
    private static readonly IReadOnlyList<WorldChampion> Champions =
    [
        new() { Id = 13, Surname = "Kasparov", Forenames = "Garry", ChampionOrder = 13 },
    ];

    [Fact]
    public async Task TryLinkPlayerAsync_UpdatesWorldChampionAndFideIds()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetPlayerByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Player { Id = 1, Surname = "Kasparov", Forenames = "Garry" });
        repo.Setup(r => r.GetWorldChampionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Champions);
        repo.Setup(r => r.GetMinGameYearForPlayerAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((short?)1995);
        repo.Setup(r => r.GetFidePlayersBySurnameAsync("Kasparov", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FidePlayer>
            {
                new() { Id = 4100018, Surname = "Kasparov", Forenames = "Garry", BirthYear = 1963 }
            });

        var wcMatcher = new Mock<IWorldChampionMatcher>();
        wcMatcher.Setup(m => m.Match("Kasparov", "Garry", Champions)).Returns(13);

        var fideMatcher = new Mock<IFidePlayerMatcher>();
        fideMatcher.Setup(m => m.Match("Kasparov", "Garry", It.IsAny<IReadOnlyList<FidePlayer>>(), 1995))
            .Returns(4100018);

        var sut = new PlayerMetadataLinkingService(repo.Object, wcMatcher.Object, fideMatcher.Object);
        var outcome = await sut.TryLinkPlayerAsync(1);

        Assert.Equal(1, outcome.PlayersProcessed);
        Assert.Equal(1, outcome.WorldChampionLinked);
        Assert.Equal(1, outcome.FideLinked);
        repo.Verify(
            r => r.UpdatePlayerMetadataLinksAsync(1, 13, 4100018, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TryLinkPlayerAsync_ClearsIncompatibleFideLink()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetPlayerByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Player { Id = 2, Surname = "Carlsen", Forenames = "Magnus", FidePlayerId = 1503014 });
        repo.Setup(r => r.GetWorldChampionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Champions);
        repo.Setup(r => r.GetMinGameYearForPlayerAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((short?)1985);
        repo.Setup(r => r.GetFidePlayersBySurnameAsync("Carlsen", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FidePlayer>
            {
                new() { Id = 1503014, Surname = "Carlsen", Forenames = "Magnus", BirthYear = 1990 }
            });
        repo.Setup(r => r.GetFidePlayerByIdAsync(1503014, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FidePlayer { Id = 1503014, Surname = "Carlsen", Forenames = "Magnus", BirthYear = 1990 });

        var wcMatcher = new Mock<IWorldChampionMatcher>();
        wcMatcher.Setup(m => m.Match(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<WorldChampion>>()))
            .Returns((int?)null);

        var fideMatcher = new Mock<IFidePlayerMatcher>();
        fideMatcher.Setup(m => m.Match(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<FidePlayer>>(), 1985))
            .Returns((int?)null);

        var sut = new PlayerMetadataLinkingService(repo.Object, wcMatcher.Object, fideMatcher.Object);
        var outcome = await sut.TryLinkPlayerAsync(2, 1985);

        Assert.Equal(1, outcome.FideCleared);
        repo.Verify(
            r => r.UpdatePlayerMetadataLinksAsync(2, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LinkAllPlayersAsync_UsesBulkQueriesAndBatchUpdate()
    {
        var players = new List<Player>
        {
            new() { Id = 1, Surname = "Kasparov", Forenames = "Garry" },
            new() { Id = 2, Surname = "Carlsen", Forenames = "Magnus", FidePlayerId = 99 },
        };

        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetWorldChampionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Champions);
        repo.Setup(r => r.GetPlayers()).ReturnsAsync(players);
        repo.Setup(r => r.GetAllPlayerMinGameYearsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<int, short> { [1] = 1995, [2] = 2001 });
        repo.Setup(r => r.GetFidePlayersForDistinctPlayerSurnamesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FidePlayer>
            {
                new() { Id = 4100018, Surname = "Kasparov", Forenames = "Garry", BirthYear = 1963 },
                new() { Id = 1503014, Surname = "Carlsen", Forenames = "Magnus", BirthYear = 1990, FideTitle = "GM" },
            });

        var wcMatcher = new WorldChampionMatcher();
        var fideMatcher = new FidePlayerMatcher();
        var sut = new PlayerMetadataLinkingService(repo.Object, wcMatcher, fideMatcher);

        var outcome = await sut.LinkAllPlayersAsync();

        Assert.Equal(2, outcome.PlayersProcessed);
        Assert.Equal(1, outcome.WorldChampionLinked);
        Assert.Equal(2, outcome.FideLinked);
        repo.Verify(r => r.GetPlayerByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(r => r.GetMinGameYearForPlayerAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(r => r.GetFidePlayersBySurnameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(
            r => r.BulkUpdatePlayerMetadataLinksAsync(
                It.Is<IReadOnlyList<PlayerMetadataLinkUpdate>>(u => u.Count == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
