using Services.FideCatalog;

namespace ServicesTests.PlayerMetadata;

public class FideRatingListReaderTests
{
    private readonly FideRatingListReader _sut = new();

    [Fact]
    public void ParseDataLine_CarlsenMagnus_ParsesFields()
    {
        var line = "1503014        Carlsen, Magnus                                              NOR M   GM                           2841  7   10 2832  0   10 2869  0   10 1990      ";
        var row = _sut.ParseDataLine(line);

        Assert.NotNull(row);
        Assert.Equal(1503014, row!.Id);
        Assert.Equal("Carlsen", row.Surname);
        Assert.Equal("Magnus", row.Forenames);
        Assert.Equal("NOR", row.Federation);
        Assert.Equal("M", row.Sex);
        Assert.Equal("GM", row.FideTitle);
        Assert.Equal((short)1990, row.BirthYear);
    }

    [Fact]
    public void ParseDataLine_NakamuraAbbreviatedForenames_Works()
    {
        var line = "2016192        Nakamura, Hikaru                                             USA M   GM                           2750  0   10 2837  0   10 2850  0   10 1987      ";
        var row = _sut.ParseDataLine(line);

        Assert.NotNull(row);
        Assert.Equal(2016192, row!.Id);
        Assert.Equal("Nakamura", row.Surname);
        Assert.Equal("Hikaru", row.Forenames);
    }

    [Fact]
    public void IsHeaderLine_DetectsColumnHeader()
    {
        Assert.True(_sut.IsHeaderLine("ID Number      Name                                                         Fed Sex Tit  WTit OTit"));
        Assert.False(_sut.IsDataLine("ID Number      Name"));
    }
}
