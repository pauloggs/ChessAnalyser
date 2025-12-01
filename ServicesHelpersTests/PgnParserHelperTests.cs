using Services.Helpers;
using FluentAssertions;
using Interfaces.DTO;

namespace ServicesHelpersTests
{
    public class PgnParserHelperTests
    {
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
            var result = PgnParserHelper.GetRawGamesFromPgnFile(testRawPgnWithNoEventInContents);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(0);
        }

        [Fact]
        public void GetRawGamesFromPgnFile_SingleRawGameReturnsSingleGame()
        {
            var testRawPgnWithSingleGame = new RawPgn()
            {
                Name = "SomeName.pgn",
                Contents = "[Event \"Some Event\"] 1. e4 e5 2. Nf3 Nc6 3. Bb5 a6"
            };

            var result = PgnParserHelper.GetRawGamesFromPgnFile(testRawPgnWithSingleGame);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public void GetRawGamesFromPgnFile_MultipleRawGamesReturnsMultipleGames()
        {
            var testRawPgnWithMultipleGames = new RawPgn()
            {
                Name = "SomeName.pgn",
                Contents = "[Event \"Game 1\"] 1. e4 e5 2. Nf3 Nc6 3. Bb5 a6 [Event \"Game 2\"] 1. d4 d5 2. c4 c6 3. Nc3 Nf6"
            };
            var result = PgnParserHelper.GetRawGamesFromPgnFile(testRawPgnWithMultipleGames);
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public void GetRawGamesFromPgnFile_GameContentsAreCorrectlyAssigned()
        {
            var testRawPgnWithMultipleGames = new RawPgn()
            {
                Name = "SomeName.pgn",
                Contents = "[Event \"Game 1\"] 1. e4 e5 2. Nf3 Nc6 3. Bb5 a6 [Event \"Game 2\"] 1. d4 d5 2. c4 c6 3. Nc3 Nf6"
            };
            var result = PgnParserHelper.GetRawGamesFromPgnFile(testRawPgnWithMultipleGames);
            result[0].Contents.Should().StartWith("[Event \"Game 1\"]");
            result[0].Contents.Should().Contain("1. e4 e5 2. Nf3 Nc6 3. Bb5 a6");
            result[1].Contents.Should().StartWith("[Event \"Game 2\"]");
            result[1].Contents.Should().Contain("1. d4 d5 2. c4 c6 3. Nc3 Nf6");
        }

        [Fact]
        public void GetRawGamesFromPgnFile_ParentPgnFileNameIsCorrectlyAssigned()
        {
            var testRawPgnWithMultipleGames = new RawPgn()
            {
                Name = "SomeName.pgn",
                Contents = "[Event \"Game 1\"] 1. e4 e5 2. Nf3 Nc6 3. Bb5 a6 [Event \"Game 2\"] 1. d4 d5 2. c4 c6 3. Nc3 Nf6"
            };
            var result = PgnParserHelper.GetRawGamesFromPgnFile(testRawPgnWithMultipleGames);
            result[0].ParentPgnFileName.Should().Be("SomeName.pgn");
            result[1].ParentPgnFileName.Should().Be("SomeName.pgn");
        }
    }
}
