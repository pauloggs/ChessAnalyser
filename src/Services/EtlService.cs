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
        IEtlProgressStore progressStore) : IEtlService
    {
        private readonly IFileHandler _fileHandler = fileHandler;
        private readonly IPgnParser _pgnParser = pgnParser;
        private readonly IPersistenceService _persistenceService = persistenceService;
        private readonly IBoardPositionService _boardPositionService = boardPositionService;
        private readonly IEtlProgressStore _progressStore = progressStore;

        public async Task LoadGamesToDatabase(string filePath, IProgress<EtlProgress>? progress = null)
        {
            var pgnFiles = _fileHandler.LoadPgnFiles(filePath);
            var totalFiles = pgnFiles.Count;
            var totalGamesProcessed = 0;

            // Per-file game counts (and total) for accurate PercentComplete, without enumerating twice
            var gamesPerFile = new List<int>(totalFiles);
            var totalGamesAllFiles = 0;
            for (var i = 0; i < pgnFiles.Count; i++)
            {
                var c = _pgnParser.GetGameCountInFile(pgnFiles[i]);
                gamesPerFile.Add(c);
                totalGamesAllFiles += c;
            }

            var gamesIteratedSoFar = 0;

            // Report immediately so progress bar shows activity before first game
            progress?.Report(new EtlProgress
            {
                CurrentFileIndex = 0,
                TotalFiles = totalFiles,
                CurrentFileName = totalFiles > 0 ? pgnFiles[0].Name : null,
                CurrentGameIndex = 0,
                TotalGamesInCurrentFile = totalFiles > 0 ? gamesPerFile[0] : 0,
                TotalGamesProcessed = 0,
                Status = "Running",
                PercentComplete = 0
            });

            void Report(int fileIndex, string? fileName, int gameIndex, int totalInFile, string status = "Running", string? message = null)
            {
                var percent = totalGamesAllFiles > 0
                    ? (int)((gamesIteratedSoFar * 100.0) / totalGamesAllFiles)
                    : (totalFiles > 0 ? (int)((fileIndex * 100.0) / totalFiles) : 0);
                if (status == "Completed")
                    percent = 100;

                progress?.Report(new EtlProgress
                {
                    CurrentFileIndex = fileIndex,
                    TotalFiles = totalFiles,
                    CurrentFileName = fileName,
                    CurrentGameIndex = gameIndex,
                    TotalGamesInCurrentFile = totalInFile,
                    TotalGamesProcessed = totalGamesProcessed,
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
                    var totalGamesInFile = gamesPerFile[fileIndex];
                    if (totalGamesInFile == 0)
                    {
                        Report(fileIndex + 1, pgnFile.Name, 0, 0);
                        continue;
                    }

                    var processedIds = (await _persistenceService.GetProcessedGameIds()).ToHashSet();
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
                                PercentComplete = totalGamesAllFiles > 0 ? (int)((gamesIteratedSoFar * 100.0) / totalGamesAllFiles) : 0
                            });
                            return;
                        }

                        gameIndex++;
                        gamesIteratedSoFar++;
                        Report(fileIndex, pgnFile.Name, gameIndex, totalGamesInFile);

                        if (processedIds.Contains(game.GameId))
                            continue;

                        var parseErrors = new List<GameParseError>();
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
                    TotalGamesProcessed = totalGamesProcessed
                });
                throw;
            }
        }
    }
}

