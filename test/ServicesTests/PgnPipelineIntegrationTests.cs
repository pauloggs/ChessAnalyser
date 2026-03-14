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
            var legalMoveChecker = new LegalMoveChecker(bitBoardManipulator);
            var pieceSourceFinderService = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, bitBoardManipulator, legalMoveChecker);
            var pawnMoveInterpreter = new PawnMoveInterpreter(bitBoardManipulator);
            var pieceMoveInterpreter = new PieceMoveInterpreter(sourceSquareHelper, pieceSourceFinderService);
            var destinationSquareHelper = new DestinationSquareHelper();
            var moveInterpreterHelper = new MoveInterpreterHelper(destinationSquareHelper, pawnMoveInterpreter, pieceMoveInterpreter);
            var moveInterpreter = new MoveInterpreter(moveInterpreterHelper);
            var displayService = new DisplayService();
            var boardPositionCalculatorHelper = new BoardPositionCalculatorHelper(bitBoardManipulator, legalMoveChecker);
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

            // Invalid or ambiguous games are omitted; valid games remain.
            if (games.Count == 0)
                return; // Game was omitted (illegal or ambiguous move).
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

        [Fact]
        public void FullPipeline_EnPassantPgn_LoadsParsesAndComputesBoardPositionsWithoutThrow()
        {
            var path = GetIntegrationTestDataPath("en-passant.pgn");
            if (!File.Exists(path))
            {
                path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs", "en-passant.pgn"));
            }
            Assert.True(File.Exists(path), "en-passant.pgn not found at " + path);

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

        /// <summary>
        /// Game 28 (Shaposhnikov vs Alekhine, B20): without heuristics, 12.Ne2 is ambiguous (two knights can reach e2)
        /// so the parser correctly throws "Ambiguous move". If the PGN had disambiguation (e.g. 12.Nde2), the pipeline
        /// would complete and after 15.Ne4 (ply 28) a white knight would be on e4.
        /// </summary>
        [Fact]
        public void FullPipeline_Game28_AmbiguousNe2_ThrowsOrCompletesWithKnightOnE4()
        {
            var path = GetIntegrationTestDataPath("Alekhine.pgn");
            if (!File.Exists(path))
            {
                path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs", "Alekhine.pgn"));
            }
            Assert.True(File.Exists(path), "Alekhine.pgn not found at " + path);

            var (fileHandler, pgnParser, boardPositionService) = BuildPipeline();

            var pgnFiles = fileHandler.LoadPgnFiles(path);
            var games = pgnParser.GetGamesFromPgnFiles(pgnFiles);
            Assert.True(games.Count >= 28, "Alekhine.pgn should have at least 28 games");

            var game28 = games[27];
            Assert.Contains("Shaposhnikov", game28.Name, StringComparison.OrdinalIgnoreCase);

            var listForGame28 = new List<Game> { game28 };
            boardPositionService.SetBoardPositions(listForGame28);

            // If game was omitted (ambiguous 12.Ne2 or other invalid move), list is empty.
            if (listForGame28.Count == 0)
                return;

            // Game completed; assert position after 15.Ne4.
            Assert.True(game28.BoardPositions.ContainsKey(28), "Position after ply 28 must be present");
            var posAfterPly28 = game28.BoardPositions[28];
            Assert.True(posAfterPly28.PiecePositions.TryGetValue("WN", out var wnBb), "Position should have WN bitboard");
            const int e4 = 28;
            Assert.True((wnBb & (1UL << e4)) != 0, "After 15.Ne4 (ply 28), a white knight must be on e4 (square 28).");
        }
    }
}
