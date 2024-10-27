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
        List<RawGame> GetRawGamesFromPgnFiles(List<RawPgn> rawPgns);
    }

    public class PgnProcessor(IParser fileSplitter) : IPgnProcessor
    {
        private readonly IParser fileSplitter = fileSplitter;

        public List<RawGame> GetRawGamesFromPgnFiles(List<RawPgn> rawPgns)
        {
            var rawGames = new List<RawGame>();

            foreach (var rawPgn in rawPgns)
            {
                var rawGamesForPgnFile = fileSplitter.GetRawGamesFromPgnFile(rawPgn);

                rawGames.AddRange(rawGamesForPgnFile);
            }

            return rawGames;
        }
    }
}
