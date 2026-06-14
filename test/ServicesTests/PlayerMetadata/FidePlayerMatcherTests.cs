using Interfaces.DTO.Ref;
using Services.PlayerMetadata;

namespace ServicesTests.PlayerMetadata;

public class FidePlayerMatcherTests
{
    private static readonly IReadOnlyList<FidePlayer> Catalog =
    [
        new() { Id = 1503014, Surname = "Carlsen", Forenames = "Magnus", Federation = "NOR", Sex = "M", FideTitle = "GM", BirthYear = 1990 },
        new() { Id = 1487710, Surname = "Carlsen", Forenames = "Alexander Joe Stucke", Federation = "DEN", Sex = "M", BirthYear = 1995 },
        new() { Id = 2016192, Surname = "Nakamura", Forenames = "Hikaru", Federation = "USA", Sex = "M", FideTitle = "GM", BirthYear = 1987 },
    ];

    private readonly FidePlayerMatcher _sut = new();

    [Fact]
    public void Match_ExactCarlsenMagnus_ReturnsOfficialId()
    {
        Assert.Equal(1503014, _sut.Match("Carlsen", "Magnus", Catalog, 2020));
    }

    [Fact]
    public void Match_AbbreviatedForenames_ReturnsMatch()
    {
        Assert.Equal(2016192, _sut.Match("Nakamura", "H.", Catalog, 2015));
    }

    [Fact]
    public void Match_HomonymCarlsen_UsesBirthYearToDisambiguate()
    {
        Assert.Equal(1503014, _sut.Match("Carlsen", "Magnus", Catalog, 2010));
        Assert.Null(_sut.Match("Carlsen", "Alexander Joe Stucke", Catalog, 1992));
    }

    [Fact]
    public void Match_AmbiguousDuplicateNames_ReturnsNull()
    {
        var ambiguous =
            new List<FidePlayer>
            {
                new() { Id = 1, Surname = "Smith", Forenames = "John", BirthYear = 1980 },
                new() { Id = 2, Surname = "Smith", Forenames = "John", BirthYear = 1980 },
            };

        Assert.Null(_sut.Match("Smith", "John", ambiguous, 2020));
    }

    [Fact]
    public void Match_KasparovGary_ReturnsGarryId()
    {
        var kasparovCatalog = new List<FidePlayer>
        {
            new() { Id = 4100018, Surname = "Kasparov", Forenames = "Garry", BirthYear = 1963, FideTitle = "GM" },
            new() { Id = 1662627, Surname = "Kasparov", Forenames = "Mark", BirthYear = 2008 },
        };

        Assert.Equal(4100018, _sut.Match("Kasparov", "Gary", kasparovCatalog, 1978));
    }

    [Fact]
    public void Match_CarlsenInitial_PrefersTitledMagnusOverMikkel()
    {
        var carlsenCatalog = new List<FidePlayer>
        {
            new() { Id = 1489054, Surname = "Carlsen", Forenames = "Mikkel", BirthYear = 1987 },
            new() { Id = 1503014, Surname = "Carlsen", Forenames = "Magnus", BirthYear = 1990, FideTitle = "GM" },
        };

        Assert.Equal(1503014, _sut.Match("Carlsen", "M", carlsenCatalog, 2001));
    }

    [Fact]
    public void Match_SingleLetterForename_SkipsWeakMatchWhenNotUnique()
    {
        var catalog = new List<FidePlayer>
        {
            new() { Id = 1, Surname = "Blanco", Forenames = "Jose", BirthYear = 1980 },
            new() { Id = 2, Surname = "Blanco", Forenames = "Juan", BirthYear = 1985 },
        };

        Assert.Null(_sut.Match("Blanco", "J", catalog, 2020));
    }

    [Fact]
    public void Match_BirthYearAfterGameYear_ReturnsNull()
    {
        Assert.Null(_sut.Match("Carlsen", "Magnus", Catalog, 1985));
    }

    [Theory]
    [InlineData((short)1990, (short)1990, true)]
    [InlineData((short)1990, (short)2000, true)]
    [InlineData((short)1990, (short)1989, false)]
    [InlineData(null, (short)1990, true)]
    public void IsBirthYearCompatible_FollowsDesignRule(short? birthYear, short gameYear, bool expected)
    {
        Assert.Equal(expected, FidePlayerMatcher.IsBirthYearCompatible(birthYear, gameYear));
    }
}
