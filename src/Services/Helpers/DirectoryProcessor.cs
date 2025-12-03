using Interfaces.DTO;

namespace Services.Helpers
{
    public static class DirectoryProcessor
    {
        /// <summary>
        /// Processes the specified directory and its subdirectories, loading files into the provided collection.
        /// </summary>
        /// <remarks>This method recursively processes all files in the specified directory and its
        /// subdirectories. Each file is loaded using the <see cref="FileHandlerHelper.LoadFile"/> method and added to
        /// the provided collection.</remarks>
        /// <param name="targetDirectory">The path of the directory to process. Must be a valid directory path.</param>
        /// <param name="rawPgnFiles">A collection to which the loaded files will be added. Cannot be <see langword="null"/>.</param>
        public static void ProcessDirectory(string targetDirectory, List<PgnFile> rawPgnFiles)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                FileHandlerHelper.LoadFile(fileName, rawPgnFiles);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory, rawPgnFiles);
        }
    }
}
