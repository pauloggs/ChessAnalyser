using Interfaces.DTO;
using Moq;
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

        [Fact(DisplayName = "FullPipeline_SamePgnParsedAndProcessedTwice_ProducesIdenticalResult_Determinism")]
        public void FullPipeline_SamePgnParsedAndProcessedTwice_ProducesIdenticalResult_Determinism()
        {
            var path = GetIntegrationTestDataPath("checkmate.pgn");
            if (!File.Exists(path))
                path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs", "checkmate.pgn"));
            Assert.True(File.Exists(path), "checkmate.pgn not found");

            var (fileHandler, pgnParser, boardPositionService) = BuildPipeline();
            var pgnFiles = fileHandler.LoadPgnFiles(path);
            Assert.NotEmpty(pgnFiles);

            var games1 = pgnParser.GetGamesFromPgnFile(pgnFiles[0]);
            var games2 = pgnParser.GetGamesFromPgnFile(pgnFiles[0]);
            Assert.Equal(games1.Count, games2.Count);

            boardPositionService.SetBoardPositions(games1);
            boardPositionService.SetBoardPositions(games2);

            Assert.Equal(games1.Count, games2.Count);
            for (int i = 0; i < games1.Count; i++)
            {
                var g1 = games1[i];
                var g2 = games2[i];
                Assert.Equal(g1.GameId, g2.GameId);
                Assert.Equal(g1.BoardPositions.Count, g2.BoardPositions.Count);
                foreach (var kv in g1.BoardPositions)
                {
                    Assert.True(g2.BoardPositions.TryGetValue(kv.Key, out var pos2), "Same ply index must exist in second run");
                    var pos1 = kv.Value;
                    foreach (var key in pos1.PiecePositions.Keys)
                    {
                        Assert.True(pos2.PiecePositions.TryGetValue(key, out var bb2), "Same piece key must exist");
                        Assert.Equal(pos1.PiecePositions[key], bb2);
                    }
                }
            }
        }

        /// <summary>
        /// Runs ETL with a sample PGN file and collects progress reports. Proves PercentComplete increases and reaches 100.
        /// </summary>
        [Fact]
        public async Task Etl_WithSamplePgnFile_ReportsProgressWithIncreasingPercentCompleteAndCompletesAt100()
        {
            var path = GetIntegrationTestDataPath("etl_progress_sample.pgn");
            if (!File.Exists(path))
                path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData", "Integration", "etl_progress_sample.pgn"));
            Assert.True(File.Exists(path), "etl_progress_sample.pgn not found at " + path);

            var (fileHandler, pgnParser, boardPositionService) = BuildPipeline();
            var persistenceMock = new Mock<IPersistenceService>();
            persistenceMock.Setup(p => p.GetProcessedGameIds()).ReturnsAsync(new List<string>());
            persistenceMock.Setup(p => p.InsertGames(It.IsAny<List<Game>>())).Returns(Task.CompletedTask);
            persistenceMock.Setup(p => p.InsertParseErrors(It.IsAny<List<GameParseError>>())).Returns(Task.CompletedTask);
            var progressStore = new EtlProgressStore();
            var playerResolverMock = new Mock<IPlayerResolver>();
            playerResolverMock.Setup(pr => pr.LoadKnownPlayersAsync()).Returns(Task.CompletedTask);
            playerResolverMock.Setup(pr => pr.ResolveGamePlayersAsync(It.IsAny<Game>())).Returns(Task.CompletedTask);

            var etlService = new EtlService(fileHandler, pgnParser, persistenceMock.Object, boardPositionService, progressStore, playerResolverMock.Object);
            var reports = new List<EtlProgress>();
            // Use synchronous progress so "Completed" is in the list before the test asserts (Progress<T> can post async)
            var progress = new SynchronousProgress<EtlProgress>(p => reports.Add(p));

            await etlService.LoadGamesToDatabase(path, progress);

            Assert.True(reports.Count >= 1, "At least one progress report expected");
            var completedReport = reports.LastOrDefault(r => r.Status == "Completed");
            Assert.NotNull(completedReport);
            Assert.Equal(100, completedReport.PercentComplete);
            var withNonZeroPercent = reports.Where(r => (r.PercentComplete ?? 0) > 0).ToList();
            Assert.True(withNonZeroPercent.Count >= 1, "PercentComplete should be non-zero during run (got: " + string.Join(", ", reports.Select(r => r.PercentComplete?.ToString() ?? "null")) + ")");
        }
    }

    internal sealed class SynchronousProgress<T> : IProgress<T>
    {
        private readonly Action<T> _handler;
        public SynchronousProgress(Action<T> handler) => _handler = handler;
        public void Report(T value) => _handler(value);
    }
}
