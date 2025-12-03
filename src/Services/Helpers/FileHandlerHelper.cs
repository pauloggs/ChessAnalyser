using Interfaces.DTO;

namespace Services.Helpers
{
    public static class FileHandlerHelper
    {
        /// <summary>
        /// Loads the contents of a file into a list of <see cref="PgnFile"/> objects.
        /// </summary>
        /// <remarks>This method reads the contents of the specified file, creates a <see cref="PgnFile"/>
        /// object containing the file name and its contents,  and adds it to the provided list. The caller is
        /// responsible for ensuring the validity of the file path and the list.</remarks>
        /// <param name="path">The full path of the file to load. Must not be <see langword="null"/> or empty.</param>
        /// <param name="rawPgnFiles">The list to which the loaded <see cref="PgnFile"/> object will be added. Must not be <see langword="null"/>.</param>
        public static void LoadFile(string path, List<PgnFile> rawPgnFiles)
        {
            // Get the file name
            var fileName = Path.GetFileName(path);

            // Read the file contents
            string fileContents = File.ReadAllText(path);

            // Create a RawPgn object and add it to the list
            var rawPgn = new PgnFile()
            {
                Name = fileName,
                Contents = fileContents
            };

            // Add to the provided list
            rawPgnFiles.Add(rawPgn);
        }
    }
}
