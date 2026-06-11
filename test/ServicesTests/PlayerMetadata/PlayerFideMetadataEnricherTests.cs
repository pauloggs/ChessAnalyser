using Interfaces.DTO;
using Moq;
using Repositories;
using Services.PlayerMetadata;

namespace ServicesTests.PlayerMetadata;

public class PlayerFideMetadataEnricherTests
{
    [Fact]
    public async Task EnrichAllAsync_UpdatesMatchedPlayerWithChangedMetadata()
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

        var wc = new Mock<IWorldChampionMatcher>();
        wc.Setup(m => m.EnsureLoadedAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        wc.Setup(m => m.IsWorldChampion(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var sut = new PlayerFideMetadataEnricher(repo.Object, matcher.Object, wc.Object);
        var result = await sut.EnrichAllAsync();

        Assert.Equal(1, result.PlayersMatched);
        Assert.Equal(1, result.PlayersUpdated);
        repo.Verify(r => r.UpdatePlayerFideMetadataAsync(
            1,
            It.Is<PlayerFideMetadata>(m => m.FideId == 1503014 && m.Federation == "NOR"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnrichAllAsync_SkipsSecondPlayerWhenFideIdAlreadyClaimed()
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

        var wc = CreateWorldChampionMatcherMock();
        var sut = new PlayerFideMetadataEnricher(repo.Object, matcher.Object, wc.Object);
        var result = await sut.EnrichAllAsync();

        Assert.Equal(2, result.PlayersMatched);
        Assert.Equal(1, result.PlayersUpdated);
        Assert.Equal(1, result.PlayersFideIdConflict);
        repo.Verify(r => r.UpdatePlayerFideMetadataAsync(2, It.IsAny<PlayerFideMetadata>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnrichAllAsync_ClearsIncompatibleStoredMetadata()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetPlayers()).ReturnsAsync(new List<Player>
        {
            new()
            {
                Id = 2340,
                Surname = "Botvinnik",
                Forenames = "Mikhail M",
                WasWorldChampion = true,
                FideId = 2_805_650,
                Federation = "ISR",
                BirthYear = 1983
            }
        });
        repo.Setup(r => r.GetPlayerCorpusActivityAsync(2340, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlayerCorpusActivity { FirstGameYear = 1925, LastGameYear = 1970 });

        var matcher = new Mock<IFidePlayerMatcher>();
        matcher.Setup(m => m.EnsureLoadedAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        matcher.Setup(m => m.Match("Botvinnik", "Mikhail M", It.IsAny<FidePlayerMatchContext>()))
            .Returns(new FidePlayerMatchResult { Outcome = FidePlayerMatchOutcome.Unmatched });

        var wc = CreateWorldChampionMatcherMock();
        wc.Setup(m => m.IsWorldChampion("Botvinnik", "Mikhail M")).Returns(true);

        var sut = new PlayerFideMetadataEnricher(repo.Object, matcher.Object, wc.Object);
        var result = await sut.EnrichAllAsync();

        Assert.Equal(1, result.PlayersUnmatched);
        Assert.Equal(1, result.PlayersUpdated);
        repo.Verify(r => r.UpdatePlayerFideMetadataAsync(
            2340,
            It.Is<PlayerFideMetadata>(m => m.FideId == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TryEnrichPlayerAsync_UsesObservedGameYearBeforeCorpusIsPersisted()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetPlayerByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Player { Id = 1, Surname = "Botvinnik", Forenames = "Mikhail M" });
        repo.Setup(r => r.GetPlayerCorpusActivityAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerCorpusActivity?)null);

        FidePlayerMatchContext? captured = null;
        var matcher = new Mock<IFidePlayerMatcher>();
        matcher.Setup(m => m.EnsureLoadedAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        matcher.Setup(m => m.Match("Botvinnik", "Mikhail M", It.IsAny<FidePlayerMatchContext>()))
            .Callback<string, string?, FidePlayerMatchContext?>((_, _, ctx) => captured = ctx)
            .Returns(new FidePlayerMatchResult { Outcome = FidePlayerMatchOutcome.Unmatched });

        var wc = CreateWorldChampionMatcherMock();
        var sut = new PlayerFideMetadataEnricher(repo.Object, matcher.Object, wc.Object);
        await sut.TryEnrichPlayerAsync(1, observedGameYear: 1963);

        Assert.Equal((short)1963, captured!.CorpusFirstGameYear);
        Assert.Equal((short)1963, captured.CorpusLastGameYear);
    }

    private static Mock<IWorldChampionMatcher> CreateWorldChampionMatcherMock()
    {
        var wc = new Mock<IWorldChampionMatcher>();
        wc.Setup(m => m.EnsureLoadedAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        wc.Setup(m => m.IsWorldChampion(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        return wc;
    }
}
