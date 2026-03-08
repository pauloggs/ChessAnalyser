using Services;

namespace ServicesTests
{
    public class NamingTests
    {
        private readonly INaming _sut = new Naming();

        [Fact]
        public void GetGameName_WhenAllTagsPresent_ReturnsFormattedString()
        {
            var tags = new Dictionary<string, string>
            {
                ["White"] = "Alice Smith",
                ["Black"] = "Bob Jones",
                ["Date"] = "2024.01.15",
                ["Event"] = "Championship",
                ["Site"] = "New York",
                ["Round"] = "1"
            };

            var result = _sut.GetGameName(tags);

            Assert.Contains("White:Alice-Smith", result);
            Assert.Contains("Black:Bob-Jones", result);
            Assert.Contains("Date:2024.01.15", result);
            Assert.Contains("Event:Championship", result);
            Assert.Contains("Site:New-York", result);
            Assert.Contains("Round:1", result);
        }

        [Fact]
        public void GetGameName_WhenTagHasSpaces_ReplacesWithHyphens()
        {
            var tags = new Dictionary<string, string>
            {
                ["White"] = "A B",
                ["Black"] = "C D",
                ["Date"] = "?",
                ["Event"] = "E",
                ["Site"] = "S",
                ["Round"] = "1"
            };

            var result = _sut.GetGameName(tags);

            Assert.Contains("White:A-B", result);
            Assert.Contains("Black:C-D", result);
        }

        [Fact]
        public void GetGameName_WhenKeyMissing_ThrowsKeyNotFoundException()
        {
            var tags = new Dictionary<string, string>
            {
                ["White"] = "A",
                ["Black"] = "B",
                ["Date"] = "?",
                ["Event"] = "E",
                ["Site"] = "S"
                // Round missing
            };

            Assert.Throws<KeyNotFoundException>(() => _sut.GetGameName(tags));
        }
    }
}
