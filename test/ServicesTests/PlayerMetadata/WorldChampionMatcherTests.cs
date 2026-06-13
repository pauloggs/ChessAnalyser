using Interfaces.DTO.Ref;
using Services.PlayerMetadata;

namespace ServicesTests.PlayerMetadata;

public class WorldChampionMatcherTests
{
    private static readonly IReadOnlyList<WorldChampion> Catalog =
    [
        new() { Id = 4, Surname = "Alekhine", Forenames = "Alexander", ChampionOrder = 4 },
        new() { Id = 7, Surname = "Smyslov", Forenames = "Vasily", ChampionOrder = 7 },
        new() { Id = 8, Surname = "Tal", Forenames = "Mikhail", ChampionOrder = 8 },
        new() { Id = 11, Surname = "Fischer", Forenames = "Bobby", ChampionOrder = 11 },
        new() { Id = 13, Surname = "Kasparov", Forenames = "Garry", ChampionOrder = 13 },
        new() { Id = 16, Surname = "Carlsen", Forenames = "Magnus", ChampionOrder = 16 },
        new() { Id = 3, Surname = "Capablanca", Forenames = "Jose Raul", ChampionOrder = 3 },
    ];

    private readonly WorldChampionMatcher _sut = new();

    [Theory]
    [InlineData("Kasparov", "Garry", 13)]
    [InlineData("Kasparov", "G.", 13)]
    [InlineData("Carlsen", "Magnus", 16)]
    [InlineData("Fischer", "Bobby", 11)]
    [InlineData("Fischer", "Robert", 11)]
    [InlineData("Fischer", "Robert James", 11)]
    [InlineData("Smyslov", "Vassily", 7)]
    [InlineData("Kasparov", "Gary", 13)]
    [InlineData("Tal", "Mihail", 8)]
    [InlineData("Capablanca", "Jose Raul", 3)]
    [InlineData("Capablanca", "J.", 3)]
    [InlineData("Alekhine", "A.", 4)]
    public void Match_ReturnsChampionId(string surname, string forenames, int expectedId)
    {
        Assert.Equal(expectedId, _sut.Match(surname, forenames, Catalog));
    }

    [Theory]
    [InlineData("Morphy", "Paul")]
    [InlineData("Kasparov", "")]
    public void Match_ReturnsNullForNonChampions(string surname, string forenames)
    {
        Assert.Null(_sut.Match(surname, forenames, Catalog));
    }
}
