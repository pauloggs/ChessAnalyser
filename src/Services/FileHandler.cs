using System.IO;

namespace Services
{
    public interface IFileHandler
    {
        /// <summary>
        /// Given the provided path, load all PGN files from that location and return a 
        /// List of RawPgn files.
        /// </summary>
        void LoadPgnFiles(string path);
    }

    public class FileHandler : IFileHandler
    {
        public void LoadPgnFiles(string path)
        {
            if (Directory.Exists(path))
            {
                // This path is a directory
                Console.WriteLine("Directory exists...");
            }
        }
    }
}
