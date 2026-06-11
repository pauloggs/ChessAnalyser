using Interfaces.DTO;
using Services.PlayerMetadata;

namespace ServicesTests.PlayerMetadata;

public class FideRatingListReaderTests
{
    private static string FixturePath =>
        Path.Combine(AppContext.BaseDirectory, "PlayerMetadata", "Fixtures", "fide_sample.txt");

    [Fact]
    public async Task ReadAsync_ParsesOfficialFixedWidthRows()
    {
        var sut = new FideRatingListReader();
        var records = await sut.ReadAsync(FixturePath);

        Assert.Equal(4, records.Count);

        var carlsen = records.Single(r => r.FideId == 1_503_014);
        Assert.Equal("Carlsen, Magnus", carlsen.Name);
        Assert.Equal("Carlsen", carlsen.Surname);
        Assert.Equal("Magnus", carlsen.Forenames);
        Assert.Equal("NOR", carlsen.Federation);
        Assert.Equal("M", carlsen.Sex);
        Assert.Equal("GM", carlsen.Title);
        Assert.Equal((short)1990, carlsen.BirthYear);

        var flagged = records.Single(r => r.FideId == 25_121_731);
        Assert.Equal((short)1987, flagged.BirthYear);
    }

    [Fact]
    public void TryParseLine_RejectsHeaderAndShortLines()
    {
        Assert.False(FideRatingListReader.TryParseLine("ID Number      Name", out _));
        Assert.False(FideRatingListReader.TryParseLine("too short", out _));
    }
}

public class FidePlayerMatcherTests
{
    [Fact]
    public void Match_ExactCarlsen_ReturnsMatched()
    {
        var records = new List<FidePlayerRecord>
        {
            new()
            {
                FideId = 1_503_014,
                Name = "Carlsen, Magnus",
                Surname = "Carlsen",
                Forenames = "Magnus",
                Federation = "NOR",
                Sex = "M",
                Title = "GM",
                BirthYear = 1990
            }
        };

        var sut = new FidePlayerMatcher();
        sut.SetRecords(records);

        var result = sut.Match("Carlsen", "Magnus");

        Assert.Equal(FidePlayerMatchOutcome.Matched, result.Outcome);
        Assert.Equal(1_503_014, result.Record!.FideId);
    }

    [Fact]
    public void Match_AbbreviatedForenames_ReturnsMatched()
    {
        var records = new List<FidePlayerRecord>
        {
            new()
            {
                FideId = 1_503_014,
                Surname = "Carlsen",
                Forenames = "Magnus",
                Name = "Carlsen, Magnus",
                BirthYear = 1990
            }
        };

        var sut = new FidePlayerMatcher();
        sut.SetRecords(records);

        var result = sut.Match("Carlsen", "M");

        Assert.Equal(FidePlayerMatchOutcome.Matched, result.Outcome);
    }

    [Fact]
    public void Match_AmbiguousSurnameForenames_UsesBirthYearHint()
    {
        var records = new List<FidePlayerRecord>
        {
            new() { FideId = 1, Surname = "Carlsen", Forenames = "Alexander", Name = "Carlsen, Alexander", BirthYear = 1990 },
            new() { FideId = 2, Surname = "Carlsen", Forenames = "Alexander A", Name = "Carlsen, Alexander A", BirthYear = 1995 }
        };

        var sut = new FidePlayerMatcher();
        sut.SetRecords(records);

        var ambiguous = sut.Match("Carlsen", "Alexander");
        Assert.Equal(FidePlayerMatchOutcome.Ambiguous, ambiguous.Outcome);

        var withBirthYear = sut.Match("Carlsen", "Alexander", new FidePlayerMatchContext { KnownBirthYear = 1995 });
        Assert.Equal(FidePlayerMatchOutcome.Matched, withBirthYear.Outcome);
        Assert.Equal(2, withBirthYear.Record!.FideId);
    }

    [Fact]
    public void Match_UnknownPlayer_ReturnsUnmatched()
    {
        var sut = new FidePlayerMatcher();
        sut.SetRecords([]);

        var result = sut.Match("Morphy", "Paul");

        Assert.Equal(FidePlayerMatchOutcome.Unmatched, result.Outcome);
        Assert.Null(result.Record);
    }

    [Fact]
    public void ScoreCandidate_CorpusYearsBoostsPlausibleCandidate()
    {
        var candidate = new FidePlayerRecord
        {
            FideId = 1,
            Surname = "Carlsen",
            Forenames = "Alexander",
            BirthYear = 1995
        };

        var score = FidePlayerMatcher.ScoreCandidate(
            candidate,
            new FidePlayerMatchContext { CorpusFirstGameYear = 2010, CorpusLastGameYear = 2020 });

        Assert.True(score >= 30);
    }
}
