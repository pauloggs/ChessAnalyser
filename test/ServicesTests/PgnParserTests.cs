using Interfaces.DTO;
using Moq;
using Services;

namespace ServicesTests
{
    public class PgnParserTests
    {
        private readonly Mock<INaming> _namingMock;
        private readonly Mock<IBoardPositionService> _boardPositionServiceMock;
        private readonly IPgnParser _sut;

        public PgnParserTests()
        {
            _namingMock = new Mock<INaming>();
            _boardPositionServiceMock = new Mock<IBoardPositionService>();
            _sut = new PgnParser(
                _namingMock.Object,
                _boardPositionServiceMock.Object);
        }

        [Fact]
        public void GetGamesFromPgnFiles_WhenPgnFileHasOneGame_ReturnsOneGameWithSourceFileNameAndGameIndex()
        {
            var pgnFiles = new List<PgnFile>
            {
                new PgnFile
                {
                    Name = "MyFile.pgn",
                    Contents = "[Event \"Test\"]\n[Site \"?\"]\n[Date \"2024.01.01\"]\n[Round \"1\"]\n[White \"A\"]\n[Black \"B\"]\n[Result \"*\"]\n\n1. e4 e5 2. Nf3 Nc6"
                }
            };

            var result = _sut.GetGamesFromPgnFiles(pgnFiles);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("MyFile.pgn", result[0].SourcePgnFileName);
            Assert.Equal(1, result[0].GameIndexInFile);
            Assert.NotNull(result[0].GameId);
            Assert.NotEmpty(result[0].GameId);
            Assert.NotNull(result[0].Plies);
            Assert.NotEmpty(result[0].Plies);
        }

        [Fact]
        public void GetGamesFromPgnFiles_WhenPgnFileHasMultipleGames_ReturnsAllGamesWithCorrectIndices()
        {
            var pgnFiles = new List<PgnFile>
            {
                new PgnFile
                {
                    Name = "TwoGames.pgn",
                    Contents = "[Event \"E1\"]\n[Site \"?\"]\n[Date \"?\"]\n[Round \"1\"]\n[White \"W\"]\n[Black \"B\"]\n[Result \"*\"]\n\n1. e4 e5 "
                             + "[Event \"E2\"]\n[Site \"?\"]\n[Date \"?\"]\n[Round \"2\"]\n[White \"W\"]\n[Black \"B\"]\n[Result \"*\"]\n\n1. d4 d5"
                }
            };

            var result = _sut.GetGamesFromPgnFiles(pgnFiles);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, g => Assert.Equal("TwoGames.pgn", g.SourcePgnFileName));
            Assert.Equal(1, result[0].GameIndexInFile);
            Assert.Equal(2, result[1].GameIndexInFile);
            Assert.All(result, g => Assert.False(string.IsNullOrEmpty(g.GameId)));
        }

        [Fact]
        public void GetGamesFromPgnFiles_WhenPgnFilesEmpty_ReturnsEmptyList()
        {
            var result = _sut.GetGamesFromPgnFiles(new List<PgnFile>());

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void SetBoardPositions_DelegatesToBoardPositionService()
        {
            var games = new List<Game> { new Game { Name = "G", Plies = new Dictionary<int, Ply>() } };

            _sut.SetBoardPositions(games);

            _boardPositionServiceMock.Verify(s => s.SetBoardPositions(games, null), Times.Once);
        }
    }
}
