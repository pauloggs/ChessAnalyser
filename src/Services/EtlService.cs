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
        IBoardPositionGenerator boardPositionGenerator) : IEtlService
    {
        private readonly IFileHandler fileHandler = fileHandler;

        private readonly IPgnParser pgnParser = pgnParser;

        private readonly IBoardPositionGenerator boardPositionGenerator1 = boardPositionGenerator;

        public async Task LoadGamesToDatabase(string filePath)
        {
            var rawPgns = fileHandler.LoadPgnFiles(filePath);

            var games = pgnParser.GetGamesFromRawPgns(rawPgns);

            var unprocessedGames = persistenceService.GetUnprocessedGames(games);

            // TODO. process each Game to the board positions - BoardPositionGenerator
            // needs to include a bit of feedback - i.e. visual display of the board! Best way to check
            boardPositionGenerator.SetBoardPositions(games);

            // write each Game to the database PersistenceService
            await persistenceService.InsertGames(unprocessedGames);
        }
    }
}
