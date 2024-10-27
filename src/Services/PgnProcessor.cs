using Interfaces.DTO;

namespace Services
{
    public interface IPgnProcessor
    {
        /// <summary>
        /// Splits each of the provided PGN file contents into separate games.
        /// </summary>
        /// <param name="rawPgns"></param>
        /// <returns></returns>
        List<RawGame> ProcessFiles(List<RawPgn> rawPgns);

        /// <summary>
        /// Splits the contents of the provided PGN file into separate games.
        /// </summary>
        /// <param name="rawPgn"></param>
        /// <returns></returns>
        List<RawGame> RetrieveGamesFromPgnFile(RawPgn rawPgn);
    }

    public class PgnProcessor(IParser fileSplitter) : IPgnProcessor
    {
        private readonly IParser fileSplitter = fileSplitter;

        public List<RawGame> ProcessFiles(List<RawPgn> rawPgns)
        {
            var rawGames = new List<RawGame>();

            foreach (var rawPgn in rawPgns)
            {
                var rawGamesForPgnFile = RetrieveGamesFromPgnFile(rawPgn);

                rawGames.AddRange(rawGamesForPgnFile);
            }

            return rawGames;
        }

        public List<RawGame> RetrieveGamesFromPgnFile(RawPgn rawPgn)
        {
            var rawGamesForPgnFile = new List<RawGame>();

            var rawPgnRawGames = fileSplitter.GetRawGamesFromPgnFile(rawPgn);

            foreach (var rawGame in rawPgnRawGames)
            {
                rawGamesForPgnFile.Add(rawGame);
            }

            return rawGamesForPgnFile;
        }
    }
}
