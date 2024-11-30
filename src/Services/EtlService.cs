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

    public class EtlService(IFileHandler fileHandler, IParser parser) : IEtlService
    {
        private readonly IFileHandler fileHandler = fileHandler;

        private readonly IParser parser = parser;

        public void LoadGamesToDatabase(string filePath)
        {
            var rawPgns = fileHandler.LoadPgnFiles(filePath);

            var games = parser.GetGamesFromRawPgns(rawPgns);

            // foreach Game, parse all the moves to
            //  1. get a GameId from the moves
            //  2. check if this has already been processed (TODO)
            //  3. if it hasn't been processed, convert the moves to board positions and persist
        }
    }
}
