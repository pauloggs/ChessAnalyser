namespace Services
{
    public interface IEtlService
    {
        /// <summary>
        /// This orchestration method takes a path to a set of PGN files, splits these
        /// into separate games, and loads thes games into the database.
        /// </summary>
        /// <param name="filePath"></param>
        void LoadGamesToDatabase(string filePath);
    }

    public class EtlService(IFileHandler fileHandler, IPgnParser pgnParser, IGameValidator gameValidator) : IEtlService
    {
        private readonly IFileHandler fileHandler = fileHandler;

        private readonly IPgnParser parser = pgnParser;

        public void LoadGamesToDatabase(string filePath)
        {
            var rawPgns = fileHandler.LoadPgnFiles(filePath);

            var games = parser.GetGamesFromRawPgns(rawPgns);

            var unprocessedGames = gameValidator.GetUnprocessedGames(games);
        }
    }
}
