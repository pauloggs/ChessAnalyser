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

        var sut = CreateSut(repo.Object, matcher.Object);
        var result = await sut.SyncWorldChampionFlagsAsync();

        Assert.Equal(3, result.PlayersChecked);
        Assert.Equal(1, result.PlayersUpdated);
        repo.Verify(r => r.UpdatePlayerWasWorldChampionAsync(1, true, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.UpdatePlayerWasWorldChampionAsync(2, It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(r => r.UpdatePlayerWasWorldChampionAsync(3, It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncFideMetadataAsync_UpdatesMatchedPlayerWithChangedMetadata()
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

        var reader = new Mock<IFideRatingListReader>();
        reader.Setup(r => r.ReadAsync("list.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FidePlayerRecord> { fideRecord });

        var matcher = new Mock<IFidePlayerMatcher>();
        matcher.Setup(m => m.Match("Carlsen", "Magnus", It.IsAny<FidePlayerMatchContext>()))
            .Returns(new FidePlayerMatchResult { Outcome = FidePlayerMatchOutcome.Matched, Record = fideRecord });

        var sut = CreateSut(repo.Object, worldChampionMatcher: null, reader.Object, matcher.Object);
        var result = await sut.SyncFideMetadataAsync("list.txt");

        Assert.Equal(1, result.PlayersChecked);
        Assert.Equal(1, result.PlayersMatched);
        Assert.Equal(1, result.PlayersUpdated);
        Assert.Equal(0, result.PlayersUnmatched);
        Assert.Equal(0, result.PlayersAmbiguous);
        Assert.False(result.DryRun);
        repo.Verify(r => r.UpdatePlayerFideMetadataAsync(
            1,
            It.Is<PlayerFideMetadata>(m =>
                m.FideId == 1503014 &&
                m.Federation == "NOR" &&
                m.Sex == "M" &&
                m.FideTitle == "GM" &&
                m.BirthYear == 1990),
            It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.UpdatePlayerWasWorldChampionAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncFideMetadataAsync_DryRun_DoesNotPersistUpdates()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetPlayers()).ReturnsAsync(new List<Player>
        {
            new() { Id = 1, Surname = "Carlsen", Forenames = "Magnus" }
        });
        repo.Setup(r => r.GetPlayerCorpusActivityAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerCorpusActivity?)null);

        var fideRecord = new FidePlayerRecord
        {
            FideId = 1503014,
            Surname = "Carlsen",
            Forenames = "Magnus",
            Federation = "NOR"
        };

        var reader = new Mock<IFideRatingListReader>();
        reader.Setup(r => r.ReadAsync("list.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FidePlayerRecord> { fideRecord });

        var matcher = new Mock<IFidePlayerMatcher>();
        matcher.Setup(m => m.Match("Carlsen", "Magnus", It.IsAny<FidePlayerMatchContext>()))
            .Returns(new FidePlayerMatchResult { Outcome = FidePlayerMatchOutcome.Matched, Record = fideRecord });

        var sut = CreateSut(repo.Object, worldChampionMatcher: null, reader.Object, matcher.Object);
        var result = await sut.SyncFideMetadataAsync("list.txt", dryRun: true);

        Assert.True(result.DryRun);
        Assert.Equal(1, result.PlayersUpdated);
        repo.Verify(r => r.UpdatePlayerFideMetadataAsync(It.IsAny<int>(), It.IsAny<PlayerFideMetadata>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncFideMetadataAsync_IdempotentWhenMetadataUnchanged()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetPlayers()).ReturnsAsync(new List<Player>
        {
            new()
            {
                Id = 1,
                Surname = "Carlsen",
                Forenames = "Magnus",
                FideId = 1503014,
                Federation = "NOR",
                Sex = "M",
                FideTitle = "GM",
                BirthYear = 1990
            }
        });
        repo.Setup(r => r.GetPlayerCorpusActivityAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerCorpusActivity?)null);

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

        var reader = new Mock<IFideRatingListReader>();
        reader.Setup(r => r.ReadAsync("list.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FidePlayerRecord> { fideRecord });

        var matcher = new Mock<IFidePlayerMatcher>();
        matcher.Setup(m => m.Match("Carlsen", "Magnus", It.IsAny<FidePlayerMatchContext>()))
            .Returns(new FidePlayerMatchResult { Outcome = FidePlayerMatchOutcome.Matched, Record = fideRecord });

        var sut = CreateSut(repo.Object, worldChampionMatcher: null, reader.Object, matcher.Object);
        var result = await sut.SyncFideMetadataAsync("list.txt");

        Assert.Equal(1, result.PlayersMatched);
        Assert.Equal(0, result.PlayersUpdated);
        repo.Verify(r => r.UpdatePlayerFideMetadataAsync(It.IsAny<int>(), It.IsAny<PlayerFideMetadata>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncFideMetadataAsync_CountsUnmatchedAndAmbiguous()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetPlayers()).ReturnsAsync(new List<Player>
        {
            new() { Id = 1, Surname = "Unknown", Forenames = "Player" },
            new() { Id = 2, Surname = "Smith", Forenames = "John" }
        });
        repo.Setup(r => r.GetPlayerCorpusActivityAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerCorpusActivity?)null);

        var reader = new Mock<IFideRatingListReader>();
        reader.Setup(r => r.ReadAsync("list.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FidePlayerRecord>());

        var matcher = new Mock<IFidePlayerMatcher>();
        matcher.Setup(m => m.Match("Unknown", "Player", It.IsAny<FidePlayerMatchContext>()))
            .Returns(new FidePlayerMatchResult { Outcome = FidePlayerMatchOutcome.Unmatched });
        matcher.Setup(m => m.Match("Smith", "John", It.IsAny<FidePlayerMatchContext>()))
            .Returns(new FidePlayerMatchResult { Outcome = FidePlayerMatchOutcome.Ambiguous });

        var sut = CreateSut(repo.Object, worldChampionMatcher: null, reader.Object, matcher.Object);
        var result = await sut.SyncFideMetadataAsync("list.txt");

        Assert.Equal(2, result.PlayersChecked);
        Assert.Equal(0, result.PlayersMatched);
        Assert.Equal(1, result.PlayersUnmatched);
        Assert.Equal(1, result.PlayersAmbiguous);
        Assert.Equal(0, result.PlayersUpdated);
        repo.Verify(r => r.UpdatePlayerFideMetadataAsync(It.IsAny<int>(), It.IsAny<PlayerFideMetadata>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncFideMetadataAsync_SkipsSecondPlayerWhenFideIdAlreadyClaimed()
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

        var reader = new Mock<IFideRatingListReader>();
        reader.Setup(r => r.ReadAsync("list.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FidePlayerRecord> { fideRecord });

        var matcher = new Mock<IFidePlayerMatcher>();
        matcher.Setup(m => m.Match(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FidePlayerMatchContext>()))
            .Returns(new FidePlayerMatchResult { Outcome = FidePlayerMatchOutcome.Matched, Record = fideRecord });

        var sut = CreateSut(repo.Object, worldChampionMatcher: null, reader.Object, matcher.Object);
        var result = await sut.SyncFideMetadataAsync("list.txt");

        Assert.Equal(2, result.PlayersMatched);
        Assert.Equal(1, result.PlayersUpdated);
        Assert.Equal(1, result.PlayersFideIdConflict);
        repo.Verify(r => r.UpdatePlayerFideMetadataAsync(1, It.IsAny<PlayerFideMetadata>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.UpdatePlayerFideMetadataAsync(2, It.IsAny<PlayerFideMetadata>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static PlayerMetadataSyncService CreateSut(
        IChessRepository repository,
        IWorldChampionMatcher? worldChampionMatcher = null,
        IFideRatingListReader? fideRatingListReader = null,
        IFidePlayerMatcher? fidePlayerMatcher = null)
    {
        worldChampionMatcher ??= Mock.Of<IWorldChampionMatcher>();
        fideRatingListReader ??= Mock.Of<IFideRatingListReader>();
        fidePlayerMatcher ??= Mock.Of<IFidePlayerMatcher>();
        return new PlayerMetadataSyncService(repository, worldChampionMatcher, fideRatingListReader, fidePlayerMatcher);
    }
}
