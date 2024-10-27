using Interfaces.DTO;

namespace Services
{
    public interface IEtlService
    {
        void LoadGamesToDatabase(string filePath);
    }

    public class EtlService(IFileHandler fileHandler, IFileSplitter fileSplitter) : IEtlService
    {
        private readonly IFileHandler fileHandler = fileHandler;
        private readonly IFileSplitter fileSplitter = fileSplitter;

        public void LoadGamesToDatabase(string filePath)
        {
            var rawPgns = fileHandler.LoadPgnFiles(filePath);

            var rawGames = ProcessPgnFiles(rawPgns);
        }

        private List<RawGame> ProcessPgnFiles(List<RawPgn> rawPgns)
        {
            var rawGames = new List<RawGame>();

            foreach (var rawPgn in rawPgns)
            {
                var rawGamesForPgnFile = RetrieveGamesFromPgnFile(rawPgn);

                rawGames.AddRange(rawGamesForPgnFile);
            }

            return rawGames;
        }

        private List<RawGame> RetrieveGamesFromPgnFile(RawPgn rawPgn)
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
