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
            var pgnFiles = fileHandler.LoadPgnFiles(filePath);

            foreach (var pgnFile in pgnFiles)
            {
                // Parse this PGN file into games
                var games = pgnParser.GetGamesFromPgnFile(pgnFile);
                if (games == null || games.Count == 0)
                    continue;

                var unprocessedGames = await persistenceService.GetUnprocessedGames(games);
                if (unprocessedGames == null || unprocessedGames.Count == 0)
                    continue;

                var parseErrors = new List<Interfaces.DTO.GameParseError>();
                boardPositionService.SetBoardPositions(unprocessedGames, parseErrors);

                await persistenceService.InsertGames(unprocessedGames);
                await persistenceService.InsertParseErrors(parseErrors);
            }
        }
    }
}
