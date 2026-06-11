using Interfaces.DTO;
using Moq;
using Repositories;
using Services.PlayerMetadata;

namespace ServicesTests.PlayerMetadata;

public class PlayerFideMetadataEnricherTests
{
    [Fact]
    public async Task BackfillAllAsync_UpdatesMatchedPlayerWithChangedMetadata()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetPlayers()).ReturnsAsync(new List<Player>
        {
            new() { Id = 1, Surname = "Carlsen", Forenames = "Magnus" }
        });
        repo.Setup(r => r.GetPlayerCorpusActivityAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlayerCorpusActivity { FirstGameYear = 2004, LastGameYear = 2024 });

        var fideRecord = new FidePlayerRecord
        {
            FideId = 1503014,
            Surname = "Carlsen",
            Forenames = "Magnus",
            Federation = "NOR",
            Sex = "M",
            Title = "GM",
            BirthYear = 1990
        };

        var matcher = new Mock<IFidePlayerMatcher>();
        matcher.Setup(m => m.EnsureLoadedAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        matcher.Setup(m => m.Match("Carlsen", "Magnus", It.IsAny<FidePlayerMatchContext>()))
            .Returns(new FidePlayerMatchResult { Outcome = FidePlayerMatchOutcome.Matched, Record = fideRecord });

        var sut = new PlayerFideMetadataEnricher(repo.Object, matcher.Object);
        var result = await sut.BackfillAllAsync();

        Assert.Equal(1, result.PlayersMatched);
        Assert.Equal(1, result.PlayersUpdated);
        repo.Verify(r => r.UpdatePlayerFideMetadataAsync(
            1,
            It.Is<PlayerFideMetadata>(m => m.FideId == 1503014 && m.Federation == "NOR"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BackfillAllAsync_SkipsSecondPlayerWhenFideIdAlreadyClaimed()
    {
        var fideRecord = new FidePlayerRecord
        {
            FideId = 2406144,
            Surname = "Smith",
            Forenames = "John",
            Federation = "USA"
        };

        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetPlayers()).ReturnsAsync(new List<Player>
        {
            new() { Id = 1, Surname = "Smith", Forenames = "John" },
            new() { Id = 2, Surname = "Smith", Forenames = "J." }
        });
        repo.Setup(r => r.GetPlayerCorpusActivityAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerCorpusActivity?)null);

        var matcher = new Mock<IFidePlayerMatcher>();
        matcher.Setup(m => m.EnsureLoadedAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        matcher.Setup(m => m.Match(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FidePlayerMatchContext>()))
            .Returns(new FidePlayerMatchResult { Outcome = FidePlayerMatchOutcome.Matched, Record = fideRecord });

        var sut = new PlayerFideMetadataEnricher(repo.Object, matcher.Object);
        var result = await sut.BackfillAllAsync();

        Assert.Equal(2, result.PlayersMatched);
        Assert.Equal(1, result.PlayersUpdated);
        Assert.Equal(1, result.PlayersFideIdConflict);
        repo.Verify(r => r.UpdatePlayerFideMetadataAsync(2, It.IsAny<PlayerFideMetadata>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TryEnrichPlayerAsync_SkipsWhenFideIdOwnedByAnotherPlayer()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetPlayerCorpusActivityAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerCorpusActivity?)null);
        repo.Setup(r => r.GetPlayerIdByFideIdAsync(2406144, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var fideRecord = new FidePlayerRecord { FideId = 2406144, Surname = "Smith", Forenames = "J." };
        var matcher = new Mock<IFidePlayerMatcher>();
        matcher.Setup(m => m.EnsureLoadedAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        matcher.Setup(m => m.Match("Smith", "J.", It.IsAny<FidePlayerMatchContext>()))
            .Returns(new FidePlayerMatchResult { Outcome = FidePlayerMatchOutcome.Matched, Record = fideRecord });

        var sut = new PlayerFideMetadataEnricher(repo.Object, matcher.Object);
        var enriched = await sut.TryEnrichPlayerAsync(2, "Smith", "J.");

        Assert.False(enriched);
        repo.Verify(r => r.UpdatePlayerFideMetadataAsync(It.IsAny<int>(), It.IsAny<PlayerFideMetadata>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
