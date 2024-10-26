using Interfaces.DTO;
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
            var rawPgnFiles = new List<RawPgn>();

            if (File.Exists(path))
            {
                // This path is a file
                LoadFile(path, rawPgnFiles);
            }
            else if (Directory.Exists(path))
            {
                // This path is a directory
                ProcessDirectory(path, rawPgnFiles);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", path);
            }
        }

        // Process all files in the directory passed in, recurse on any directories
        // that are found, and process the files they contain.
        public static void ProcessDirectory(string targetDirectory, List<RawPgn> rawPgnFiles)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                LoadFile(fileName, rawPgnFiles);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory, rawPgnFiles);
        }

        public static void LoadFile(string path, List<RawPgn> rawPgnFiles)
        {   
            var fileName = Path.GetFileName(path);

            string fileContents = File.ReadAllText(path);

            var rawPgn = new RawPgn()
            {
                FileName = fileName,
                FileContents = fileContents
            };

            rawPgnFiles.Add(rawPgn);

            Console.WriteLine("Loaded file '{0}'.", fileName);
        }
    }
}
