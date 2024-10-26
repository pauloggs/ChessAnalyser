namespace Services
{
    public interface IEtlService
    {
        void LoadFilesToDatabase(string filePath);
    }

    public class EtlService(IFileHandler fileHandler) : IEtlService
    {
        private readonly IFileHandler fileHandler = fileHandler;

        public void LoadFilesToDatabase(string filePath)
        {
            var rawPgns = fileHandler.LoadPgnFiles(filePath);
        }
    }
}
