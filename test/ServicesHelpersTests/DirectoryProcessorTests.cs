using Interfaces.DTO;
using Services.Helpers;

namespace ServicesHelpersTests
{
    public class DirectoryProcessorTests
    {
        [Fact]
        public void ProcessDirectory_WhenDirectoryHasOneFile_LoadsIt()
        {
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var file1 = Path.Combine(dir, "a.pgn");
                File.WriteAllText(file1, "content a");
                var list = new List<PgnFile>();

                DirectoryProcessor.ProcessDirectory(dir, list);

                Assert.Single(list);
                Assert.Equal("a.pgn", list[0].Name);
                Assert.Equal("content a", list[0].Contents);
            }
            finally
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void ProcessDirectory_WhenDirectoryHasSubdirectory_LoadsFilesRecursively()
        {
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var sub = Path.Combine(dir, "sub");
            Directory.CreateDirectory(sub);
            try
            {
                File.WriteAllText(Path.Combine(dir, "root.pgn"), "root");
                File.WriteAllText(Path.Combine(sub, "nested.pgn"), "nested");
                var list = new List<PgnFile>();

                DirectoryProcessor.ProcessDirectory(dir, list);

                Assert.Equal(2, list.Count);
                var names = list.Select(f => f.Name).OrderBy(x => x).ToList();
                Assert.Equal(["nested.pgn", "root.pgn"], names);
            }
            finally
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void ProcessDirectory_WhenDirectoryEmpty_AddsNothing()
        {
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var list = new List<PgnFile>();

                DirectoryProcessor.ProcessDirectory(dir, list);

                Assert.Empty(list);
            }
            finally
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
        }
    }
}
