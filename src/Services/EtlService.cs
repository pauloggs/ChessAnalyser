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
            List<Interfaces.DTO.Game> games = pgnParser.GetGamesFromPgnFiles(pgnFiles);

            // Filter out games that are already processed and persisted
            var unprocessedGames = await persistenceService.GetUnprocessedGames(games);

            if (unprocessedGames == null || unprocessedGames.Count == 0)
            {
                Console.WriteLine("No new games to process.");
                return;
            }

            // Generate board positions for each unprocessed game
            boardPositionService.SetBoardPositions(unprocessedGames);

            // Insert unprocessed games into the database
            await persistenceService.InsertGames(unprocessedGames);
        }
    }
}
