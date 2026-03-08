using Services.Helpers;

namespace ServicesHelpersTests
{
    public class StringExtensionsTests
    {
        [Fact]
        public void RemoveLineEndings_WhenContainsNewline_ReplacesWithSpace()
        {
            var input = "line1\nline2";
            Assert.Equal("line1 line2", input.RemoveLineEndings());
        }

        [Fact]
        public void RemoveLineEndings_WhenContainsCarriageReturnNewline_ReplacesWithSpace()
        {
            var input = "line1\r\nline2";
            Assert.Equal("line1 line2", input.RemoveLineEndings());
        }

        [Fact]
        public void RemoveLineEndings_WhenContainsMixedLineEndings_ReplacesAllWithSpaces()
        {
            var input = "a\nb\r\nc";
            Assert.Equal("a b c", input.RemoveLineEndings());
        }

        [Fact]
        public void RemoveLineEndings_WhenEmpty_ReturnsEmpty()
        {
            Assert.Equal("", "".RemoveLineEndings());
        }

        [Fact]
        public void RemoveLineEndings_WhenNoLineEndings_ReturnsSameContent()
        {
            var input = "no line endings here";
            Assert.Equal(input, input.RemoveLineEndings());
        }
    }
}
