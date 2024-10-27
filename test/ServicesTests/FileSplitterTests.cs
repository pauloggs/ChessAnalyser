using FluentAssertions;
using Interfaces.DTO;
using Services;

namespace ServicesTests
{
    public class FileSplitterTests
    {
        private readonly IParser _sut;

        public FileSplitterTests()
        {
            _sut = new Parser();
        }

        [Fact]
        public void GetRawGamesFromPgnFile_ShouldSplitSourcePgnFileCorrectly()
        {
            // Arrange
            var sourcePath = "TestData\\";
            var rawPgnFile = "test_pgn.pgn";
            var expectedRawPgn1 = "test_pgn_expected_1.pgn";
            var expectedRawPgn2 = "test_pgn_expected_2.pgn";

            var rawPgnPath = $"{sourcePath}{rawPgnFile}";
            var expectedRawGameContentPath1 = $"{sourcePath}{expectedRawPgn1}";
            var expectedRawGameContentPath2 = $"{sourcePath}{expectedRawPgn2}";

            var testPgnContents = File.ReadAllText(rawPgnPath) ?? "";

            var testRawPgn = new RawPgn()
            {
                Name = "test_pgn.pgn",
                Contents = testPgnContents
            };

            var expectedRawGameContents1 = File.ReadAllText(expectedRawGameContentPath1) ?? "";
            var expectedRawGameContents2 = File.ReadAllText(expectedRawGameContentPath2) ?? "";

            // Act
            var result = _sut.GetRawGamesFromPgnFile(testRawPgn);

            // Assert
            result.Should().HaveCount(2);
            result[0].Contents.Should().Be(expectedRawGameContents1);
            result[1].Contents.Should().Be(expectedRawGameContents2);
        }

        [Fact]
        public void GetRawGamesFromPgnFile_ShouldReturnEmptyListIfNoEventFound()
        {
            // Arrange
            var testRawPgnWithNoEventInContents = new RawPgn()
            {
                Name = "SomeName.pgn",
                Contents = "SomeContents"
            };

            // Act
            var result = _sut.GetRawGamesFromPgnFile(testRawPgnWithNoEventInContents);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(0);
        }
    }
}
