using Services.Helpers;

namespace ServicesTests.Helpers
{
    public class PlayerNameParserTests
    {
        [Fact]
        public void Parse_WithComma_SplitsSurnameAndForenames()
        {
            PlayerNameParser.Parse("Alekhine, Alexander A", out var surname, out var forenames);
            Assert.Equal("Alekhine", surname);
            Assert.Equal("Alexander A", forenames);
        }

        [Fact]
        public void Parse_WithComma_SingleForename()
        {
            PlayerNameParser.Parse("Giese, A", out var surname, out var forenames);
            Assert.Equal("Giese", surname);
            Assert.Equal("A", forenames);
        }

        [Fact]
        public void Parse_WithoutComma_FirstWordIsSurnameRestIsForenames()
        {
            PlayerNameParser.Parse("Alekhine Alexander A", out var surname, out var forenames);
            Assert.Equal("Alekhine", surname);
            Assert.Equal("Alexander A", forenames);
        }

        [Fact]
        public void Parse_SingleWord_NoForenames()
        {
            PlayerNameParser.Parse("Giese", out var surname, out var forenames);
            Assert.Equal("Giese", surname);
            Assert.Equal("", forenames);
        }

        [Fact]
        public void Parse_TrimsLeadingAndTrailingSpaces()
        {
            PlayerNameParser.Parse("  Giese , A  ", out var surname, out var forenames);
            Assert.Equal("Giese", surname);
            Assert.Equal("A", forenames);
        }

        [Fact]
        public void Parse_NullOrEmpty_ReturnsEmptyStrings()
        {
            PlayerNameParser.Parse(null, out var s1, out var f1);
            Assert.Equal("", s1);
            Assert.Equal("", f1);

            PlayerNameParser.Parse("", out var s2, out var f2);
            Assert.Equal("", s2);
            Assert.Equal("", f2);

            PlayerNameParser.Parse("   ", out var s3, out var f3);
            Assert.Equal("", s3);
            Assert.Equal("", f3);
        }
    }
}
