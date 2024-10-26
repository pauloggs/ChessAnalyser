namespace Services
{
    public interface IFileHandler
    {
        /// <summary>
        /// Given the provided path, load all PGN files from that location and return a 
        /// List of RawPgn files.
        /// </summary>
        void LoadPgnFiles();
    }

    public class FileHandler : IFileHandler
    {
        public void LoadPgnFiles()
        {
            throw new NotImplementedException("LoadPgnFiles not implemented.");
        }
    }
}
