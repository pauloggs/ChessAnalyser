using Interfaces.DTO;
using Services;
using Services.Helpers;

namespace ServicesTests
{
    public class FileHandlerTests
    {
        private readonly IFileHandler _sut = new FileHandler();

        [Fact]
        public void LoadPgnFiles_WhenPathIsFile_ReturnsListWithOnePgnFile()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "TestData", "test_pgn.pgn");
            if (!File.Exists(path))
            {
                // Fallback: create temp file for test
                path = Path.Combine(Path.GetTempPath(), "FileHandlerTests_LoadPgnFiles_File.pgn");
                File.WriteAllText(path, "[Event \"E\"]\n1. e4 e5");
            }

            var result = _sut.LoadPgnFiles(path);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result[0].Name.EndsWith(".pgn", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(result[0].Contents);
            Assert.Contains("[Event", result[0].Contents);
        }

        [Fact]
        public void LoadPgnFiles_WhenPathIsDirectory_ReturnsListOfPgnFilesFromDirectory()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "FileHandlerTests_" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(tempDir);
            try
            {
                var pgnPath = Path.Combine(tempDir, "game.pgn");
                File.WriteAllText(pgnPath, "[Event \"E\"]\n1. d4 d5");

                var result = _sut.LoadPgnFiles(tempDir);

                Assert.NotNull(result);
                Assert.NotEmpty(result);
                Assert.All(result, pgn =>
                {
                    Assert.NotNull(pgn.Name);
                    Assert.NotNull(pgn.Contents);
                });
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void LoadPgnFiles_WhenPathDoesNotExist_ReturnsEmptyList()
        {
            var path = Path.Combine(Path.GetTempPath(), "NonExistentFolder_", Guid.NewGuid().ToString("N") + ".pgn");
            Assert.False(File.Exists(path));
            Assert.False(Directory.Exists(path));

            var result = _sut.LoadPgnFiles(path);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void LoadPgnFiles_WhenPathIsInvalid_ReturnsEmptyList()
        {
            var result = _sut.LoadPgnFiles("C:\\NonExistent\\NoSuchPath.pgn");

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
