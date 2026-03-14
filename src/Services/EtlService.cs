using Interfaces.DTO;

namespace Services
{
    public interface IEtlService
    {
        /// <summary>
        /// Loads PGN files from the path, parses and persists one game at a time to minimize memory.
        /// </summary>
        /// <param name="filePath">Path to a file or directory containing PGN files.</param>
        /// <param name="progress">Optional. Report progress for progress bar / polling.</param>
        Task LoadGamesToDatabase(string filePath, IProgress<EtlProgress>? progress = null);
    }

    public class EtlService(
        IFileHandler fileHandler,
        IPgnParser pgnParser,
        IPersistenceService persistenceService,
        IBoardPositionService boardPositionService,
        IEtlProgressStore progressStore,
        IPlayerResolver playerResolver) : IEtlService
    {
        private readonly IFileHandler _fileHandler = fileHandler;
        private readonly IPgnParser _pgnParser = pgnParser;
        private readonly IPersistenceService _persistenceService = persistenceService;
        private readonly IBoardPositionService _boardPositionService = boardPositionService;
        private readonly IEtlProgressStore _progressStore = progressStore;
        private readonly IPlayerResolver _playerResolver = playerResolver;

        public async Task LoadGamesToDatabase(string filePath, IProgress<EtlProgress>? progress = null)
        {
            var pgnFiles = _fileHandler.LoadPgnFiles(filePath);
            var totalFiles = pgnFiles.Count;
            var totalGamesProcessed = 0;

            // Report immediately so progress bar shows activity
            progress?.Report(new EtlProgress
            {
                CurrentFileIndex = 0,
                TotalFiles = totalFiles,
                TotalGamesToProcess = 0,
                TotalGamesProcessed = 0,
                Status = "Running",
                Message = "Counting unprocessed games…",
                PercentComplete = 0
            });

            // First pass: count how many games are unprocessed (not already in DB). Same game in multiple files counted once.
            var processedIds = (await _persistenceService.GetProcessedGameIds()).ToHashSet();
            var totalUnprocessed = 0;
            foreach (var pgnFile in pgnFiles)
            {
                if (_progressStore.IsCancellationRequested)
                {
                    progress?.Report(new EtlProgress { Status = "Cancelled", Message = "Load cancelled by user." });
                    return;
                }
                foreach (var game in _pgnParser.EnumerateGamesFromPgnFile(pgnFile))
                {
                    if (!processedIds.Contains(game.GameId))
                    {
                        totalUnprocessed++;
                        processedIds.Add(game.GameId);
                    }
                }
            }

            // Reload processed IDs from DB for the process pass (first pass only counted, did not persist)
            processedIds = (await _persistenceService.GetProcessedGameIds()).ToHashSet();

            if (totalUnprocessed == 0)
            {
                progress?.Report(new EtlProgress
                {
                    TotalFiles = totalFiles,
                    TotalGamesToProcess = 0,
                    TotalGamesProcessed = 0,
                    Status = "Completed",
                    Message = "All games already processed.",
                    PercentComplete = 100
                });
                return;
            }

            progress?.Report(new EtlProgress
            {
                TotalFiles = totalFiles,
                TotalGamesToProcess = totalUnprocessed,
                TotalGamesProcessed = 0,
                Status = "Running",
                PercentComplete = 0
            });

            await _playerResolver.LoadKnownPlayersAsync();

            void Report(int fileIndex, string? fileName, int gameIndex, int totalInFile, string status = "Running", string? message = null)
            {
                var percent = totalUnprocessed > 0 && status == "Running"
                    ? (int)((totalGamesProcessed * 100.0) / totalUnprocessed)
                    : status == "Completed" ? 100 : 0;

                progress?.Report(new EtlProgress
                {
                    CurrentFileIndex = fileIndex,
                    TotalFiles = totalFiles,
                    CurrentFileName = fileName,
                    CurrentGameIndex = gameIndex,
                    TotalGamesInCurrentFile = totalInFile,
                    TotalGamesProcessed = totalGamesProcessed,
                    TotalGamesToProcess = totalUnprocessed,
                    Status = status,
                    Message = message,
                    PercentComplete = percent
                });
            }

            try
            {
                for (var fileIndex = 0; fileIndex < pgnFiles.Count; fileIndex++)
                {
                    var pgnFile = pgnFiles[fileIndex];
                    var totalGamesInFile = _pgnParser.GetGameCountInFile(pgnFile);
                    if (totalGamesInFile == 0)
                    {
                        Report(fileIndex + 1, pgnFile.Name, 0, 0);
                        continue;
                    }

                    var gameIndex = 0;

                    foreach (var game in _pgnParser.EnumerateGamesFromPgnFile(pgnFile))
                    {
                        if (_progressStore.IsCancellationRequested)
                        {
                            progress?.Report(new EtlProgress
                            {
                                Status = "Cancelled",
                                Message = "Load cancelled by user.",
                                TotalGamesProcessed = totalGamesProcessed,
                                TotalGamesToProcess = totalUnprocessed,
                                PercentComplete = totalUnprocessed > 0 ? (int)((totalGamesProcessed * 100.0) / totalUnprocessed) : 0
                            });
                            return;
                        }

                        gameIndex++;
                        Report(fileIndex, pgnFile.Name, gameIndex, totalGamesInFile);

                        if (processedIds.Contains(game.GameId))
                            continue;

                        await _playerResolver.ResolveGamePlayersAsync(game);
                        var parseErrors = new List<GameParseError>();
                        if (game.WhitePlayerId is null || game.BlackPlayerId is null)
                        {
                            parseErrors.Add(new GameParseError
                            {
                                SourcePgnFileName = game.SourcePgnFileName,
                                GameIndexInFile = game.GameIndexInFile,
                                GameName = game.Name,
                                ErrorMessage = "Missing or invalid White/Black player (PGN tags). Game not persisted."
                            });
                            await _persistenceService.InsertParseErrors(parseErrors);
                            continue;
                        }

                        var list = new List<Game> { game };
                        _boardPositionService.SetBoardPositions(list, parseErrors);

                        if (list.Count > 0)
                        {
                            await _persistenceService.InsertGames(list);
                            totalGamesProcessed++;
                            processedIds.Add(list[0].GameId);
                        }

                        await _persistenceService.InsertParseErrors(parseErrors);
                    }
                }

                Report(totalFiles, null, 0, 0, "Completed", null);
            }
            catch (Exception ex)
            {
                progress?.Report(new EtlProgress
                {
                    Status = "Failed",
                    Message = ex.Message,
                    TotalGamesProcessed = totalGamesProcessed,
                    TotalGamesToProcess = totalUnprocessed
                });
                throw;
            }
        }
    }
}

