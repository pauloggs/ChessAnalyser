using Interfaces.DTO;

namespace Services
{
    public interface IEtlService
    {
        void LoadFilesToDatabase(string filePath);
    }

    public class EtlService(IFileHandler fileHandler, IFileSplitter fileSplitter) : IEtlService
    {
        private readonly IFileHandler fileHandler = fileHandler;
        private readonly IFileSplitter fileSplitter = fileSplitter;

        public void LoadFilesToDatabase(string filePath)
        {
            var rawPgns = fileHandler.LoadPgnFiles(filePath);

            var rawGames = new List<RawGame>();

            foreach (var rawPgn in rawPgns)
            {
                var rawPgnFileName = rawPgn.Name;

                var rawPgnRawGames = fileSplitter.GetRawGamesFromPgnFile(rawPgn);

                foreach (var rawGame in rawPgnRawGames)
                {
                    rawGames.Add(rawGame);
                }
            }
        }
    }
}
