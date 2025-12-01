namespace Services
{
    public interface IEtlService
    {
        /// <summary>
        /// This orchestration method takes a path to a set of PGN files, splits these
        /// into separate games, and loads thes games into the database.
        /// </summary>
        /// <param name="filePath"></param>
        Task LoadGamesToDatabase(string filePath);
    }

    public class EtlService(
        IFileHandler fileHandler,
        IPgnParser pgnParser,
        IPersistenceService persistenceService,
        IBoardPositionService boardPositionGenerator) : IEtlService
    {
        private readonly IFileHandler fileHandler = fileHandler;

        private readonly IPgnParser pgnParser = pgnParser;

        private readonly IBoardPositionService boardPositionService = boardPositionGenerator;

        public async Task LoadGamesToDatabase(string filePath)
        {
            // Load PGN files from the provided path
            var pgnFiles = fileHandler.LoadPgnFiles(filePath);

            // Parse PGN files into individual games
            var games = pgnParser.GetGamesFromPgnFiles(pgnFiles);

            var unprocessedGames = persistenceService.GetUnprocessedGames(games);

            boardPositionService.SetBoardPositions(unprocessedGames);

            await persistenceService.InsertGames(unprocessedGames);
        }
    }
}
