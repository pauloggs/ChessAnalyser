using Services.PlayerMetadata;

namespace ServicesTests.PlayerMetadata;

public class WorldChampionCatalogTests
{
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
    public void IsWorldChampion_MatchesCuratedChampions(string surname, string forenames, bool expected)
    {
        Assert.Equal(expected, WorldChampionCatalog.IsWorldChampion(surname, forenames));
    }
}
