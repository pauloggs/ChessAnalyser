using Interfaces.DTO;
using Services.Helpers;

namespace ServicesTests.Helpers
{
    public class PgnGameHeaderMapperTests
    {
        private static Game MinimalGame(Dictionary<string, string>? tags = null) =>
            new()
            {
                Name = "test-game",
                Tags = tags ?? [],
                Plies = [],
                BoardPositions = []
            };

        [Theory]
        [InlineData("????.??.??", null)]
        [InlineData("1934.01.01", (short)1934)]
        [InlineData("1934.??.??", (short)1934)]
        [InlineData("1934", null)]
        [InlineData("34.01.01", null)]
        [InlineData("193?.01.01", null)]
        [InlineData("", null)]
        public void ParseGameYearFromPgnDate_ReturnsExpectedYear(string? dateTag, short? expectedYear)
        {
            Assert.Equal(expectedYear, PgnGameHeaderMapper.ParseGameYearFromPgnDate(dateTag));
        }

        [Fact]
        public void ApplyFromTags_MapsStandardHeaders()
        {
            var game = MinimalGame(new Dictionary<string, string>
            {
                ["event"] = "  Wijk aan Zee  ",
                ["site"] = "NED",
                ["date"] = "2001.12.31",
                ["eco"] = "C33"
            });

            PgnGameHeaderMapper.ApplyFromTags(game);

            Assert.Equal("Wijk aan Zee", game.Event);
            Assert.Equal("NED", game.Site);
            Assert.Equal("2001.12.31", game.DateTag);
            Assert.Equal((short)2001, game.GameYear);
            Assert.Equal("C33", game.Eco);
        }

        [Fact]
        public void ApplyFromTags_TruncatesEcoToSixteenCharacters()
        {
            var longEco = new string('A', 24);
            var game = MinimalGame(new Dictionary<string, string> { ["eco"] = longEco });

            PgnGameHeaderMapper.ApplyFromTags(game);

            Assert.Equal(16, game.Eco!.Length);
            Assert.Equal(new string('A', 16), game.Eco);
        }

        [Fact]
        public void ApplyFromTags_MissingTags_YieldsNullAnalyticsFields()
        {
            var game = MinimalGame([]);

            PgnGameHeaderMapper.ApplyFromTags(game);

            Assert.Null(game.Event);
            Assert.Null(game.Site);
            Assert.Null(game.DateTag);
            Assert.Null(game.GameYear);
            Assert.Null(game.Eco);
        }
    }
}
