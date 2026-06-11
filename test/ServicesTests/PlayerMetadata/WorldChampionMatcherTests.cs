using Interfaces.DTO;
using Moq;
using Repositories;
using Services.PlayerMetadata;

namespace ServicesTests.PlayerMetadata;

public class WorldChampionMatcherTests
{
    private static readonly WorldChampionRef[] SampleChampions =
    [
        new() { Surname = "Kasparov", Forenames = "Garry", ChampionOrder = 13 },
        new() { Surname = "Carlsen", Forenames = "Magnus", ChampionOrder = 16 },
        new() { Surname = "Fischer", Forenames = "Bobby", ChampionOrder = 11 },
        new() { Surname = "Fischer", Forenames = "Robert James", ChampionOrder = 11 },
        new() { Surname = "Capablanca", Forenames = "Jose Raul", ChampionOrder = 3 },
        new() { Surname = "Tal", Forenames = "Mikhail", ChampionOrder = 8 },
        new() { Surname = "Ding", Forenames = "Liren", ChampionOrder = 17 },
        new() { Surname = "Gukesh", Forenames = "Dommaraju", ChampionOrder = 18 },
    ];

    [Theory]
    [InlineData("Kasparov", "Garry", true)]
    [InlineData("Carlsen", "Magnus", true)]
    [InlineData("Fischer", "Bobby", true)]
    [InlineData("Fischer", "Robert", true)]
    [InlineData("Capablanca", "Jose Raul", true)]
    [InlineData("Tal", "Mikhail", true)]
    [InlineData("Ding", "Liren", true)]
    [InlineData("Gukesh", "Dommaraju", true)]
    [InlineData("Morphy", "Paul", false)]
    [InlineData("Kasparov", "", false)]
    public async Task IsWorldChampion_MatchesReferenceRows(string surname, string forenames, bool expected)
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetWorldChampions(It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleChampions);

        var sut = new WorldChampionMatcher(repo.Object);
        await sut.EnsureLoadedAsync();

        Assert.Equal(expected, sut.IsWorldChampion(surname, forenames));
    }
}
