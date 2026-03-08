using Interfaces.DTO;
using Services;
using Services.Helpers;

namespace ServicesTests
{
    /// <summary>
    /// Integration tests: load real PGN files and run the full pipeline (FileHandler → PgnParser → BoardPositionService)
    /// with no mocks. Verifies that parsing and board-position calculation complete for targeted edge-case PGNs.
    /// </summary>
    public class PgnPipelineIntegrationTests
    {
        private static (IFileHandler fileHandler, IPgnParser pgnParser, IBoardPositionService boardPositionService) BuildPipeline()
        {
            var bitBoardHelper = new BitBoardManipulatorHelper();
            var bitBoardManipulator = new BitBoardManipulator(bitBoardHelper);
            var sourceSquareHelper = new SourceSquareHelper(bitBoardManipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var pieceSourceFinderService = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, bitBoardManipulator);
            var pawnMoveInterpreter = new PawnMoveInterpreter(bitBoardManipulator);
            var pieceMoveInterpreter = new PieceMoveInterpreter(sourceSquareHelper, pieceSourceFinderService);
            var destinationSquareHelper = new DestinationSquareHelper();
            var moveInterpreterHelper = new MoveInterpreterHelper(destinationSquareHelper, pawnMoveInterpreter, pieceMoveInterpreter);
            var moveInterpreter = new MoveInterpreter(moveInterpreterHelper);
            var displayService = new DisplayService();
            var boardPositionCalculatorHelper = new BoardPositionCalculatorHelper(bitBoardManipulator);
            var boardPositionCalculator = new BoardPositionCalculator(boardPositionCalculatorHelper);
            var boardPositionsHelper = new BoardPositionsHelper(moveInterpreter, displayService, boardPositionCalculator);
            var boardPositionService = new BoardPositionService(boardPositionsHelper);
            var naming = new Naming();
            var pgnParser = new PgnParser(naming, boardPositionService);
            var fileHandler = new FileHandler();
            return (fileHandler, pgnParser, boardPositionService);
        }

        private static string GetIntegrationTestDataPath(string fileName)
        {
            return Path.Combine(AppContext.BaseDirectory, "TestData", "Integration", fileName);
        }

        [Fact]
        public void FullPipeline_CheckmatePgn_LoadsParsesAndComputesBoardPositionsWithoutThrow()
        {
            var path = GetIntegrationTestDataPath("checkmate.pgn");
            if (!File.Exists(path))
            {
                // Fallback if not copied (e.g. run from IDE with different output dir)
                path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs", "checkmate.pgn");
                path = Path.GetFullPath(path);
            }
            Assert.True(File.Exists(path), "checkmate.pgn not found at " + path);

            var (fileHandler, pgnParser, boardPositionService) = BuildPipeline();

            var pgnFiles = fileHandler.LoadPgnFiles(path);
            Assert.NotEmpty(pgnFiles);

            var games = pgnParser.GetGamesFromPgnFiles(pgnFiles);
            Assert.Single(games);

            boardPositionService.SetBoardPositions(games);

            var game = games[0];
            Assert.NotNull(game.InitialBoardPosition);
            Assert.NotNull(game.BoardPositions);
            Assert.True(game.BoardPositions.Count >= 1);
            Assert.Equal("W", game.Winner);
        }

        [Fact]
        public void FullPipeline_KingSideCastlingWithNumericsPgn_LoadsParsesAndComputesBoardPositionsWithoutThrow()
        {
            var path = GetIntegrationTestDataPath("king-side-castling-with-numerics.pgn");
            if (!File.Exists(path))
            {
                path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs", "king-side-castling-with-numerics.pgn"));
            }
            Assert.True(File.Exists(path), "king-side-castling-with-numerics.pgn not found at " + path);

            var (fileHandler, pgnParser, boardPositionService) = BuildPipeline();

            var pgnFiles = fileHandler.LoadPgnFiles(path);
            Assert.NotEmpty(pgnFiles);

            var games = pgnParser.GetGamesFromPgnFiles(pgnFiles);
            Assert.Single(games);

            boardPositionService.SetBoardPositions(games);

            var game = games[0];
            Assert.NotNull(game.InitialBoardPosition);
            Assert.True(game.BoardPositions.Count >= 1);
            Assert.Equal("B", game.Winner);
        }

        [Fact]
        public void FullPipeline_PromotionPgn_LoadsParsesAndComputesBoardPositionsWithoutThrow()
        {
            var path = GetIntegrationTestDataPath("promotion.pgn");
            if (!File.Exists(path))
            {
                path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs", "promotion.pgn"));
            }
            Assert.True(File.Exists(path), "promotion.pgn not found at " + path);

            var (fileHandler, pgnParser, boardPositionService) = BuildPipeline();

            var pgnFiles = fileHandler.LoadPgnFiles(path);
            Assert.NotEmpty(pgnFiles);

            var games = pgnParser.GetGamesFromPgnFiles(pgnFiles);
            Assert.Single(games);

            boardPositionService.SetBoardPositions(games);

            var game = games[0];
            Assert.NotNull(game.InitialBoardPosition);
            Assert.True(game.BoardPositions.Count >= 1);
            Assert.Equal("W", game.Winner);
        }

        [Fact]
        public void FullPipeline_QueenSideCastlingWithNumericsPgn_LoadsParsesAndComputesBoardPositionsWithoutThrow()
        {
            var path = GetIntegrationTestDataPath("queen-side-castling-with-numerics.pgn");
            if (!File.Exists(path))
            {
                path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs", "queen-side-castling-with-numerics.pgn"));
            }
            Assert.True(File.Exists(path), "queen-side-castling-with-numerics.pgn not found at " + path);

            var (fileHandler, pgnParser, boardPositionService) = BuildPipeline();

            var pgnFiles = fileHandler.LoadPgnFiles(path);
            Assert.NotEmpty(pgnFiles);

            var games = pgnParser.GetGamesFromPgnFiles(pgnFiles);
            Assert.Single(games);

            boardPositionService.SetBoardPositions(games);

            var game = games[0];
            Assert.NotNull(game.InitialBoardPosition);
            Assert.True(game.BoardPositions.Count >= 1);
            Assert.Equal("B", game.Winner);
        }
    }
}
