using Interfaces.DTO;
using Services.Helpers;

namespace Services
{
    public interface IFileHandler
    {
        /// <summary>
        /// Given the provided path, load all PGN files from that location and return a 
        /// List of RawPgn files.
        /// </summary>
        List<RawPgn> LoadPgnFiles(string path);
    }

    public class FileHandler : IFileHandler
    {
        public List<RawPgn> LoadPgnFiles(string path)
        {
            var rawPgnFiles = new List<RawPgn>();

            if (File.Exists(path))
            {
                // This path is a file
                FileHandlerHelper.LoadFile(path, rawPgnFiles);
            }
            else if (Directory.Exists(path))
            {
                // This path is a directory
                DirectoryProcessor.ProcessDirectory(path, rawPgnFiles);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", path);
            }

            return rawPgnFiles ?? [];
        }       
    }
}
