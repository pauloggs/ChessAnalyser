using Interfaces.DTO;
using Services.Helpers;

namespace ServicesHelpersTests
{
    public class FileHandlerHelperTests
    {
        [Fact]
        public void LoadFile_WhenFileExists_AddsPgnFileToListWithContents()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var content = "[Event \"Test\"]\n1. e4 e5";
                File.WriteAllText(tempFile, content);
                var list = new List<PgnFile>();

                FileHandlerHelper.LoadFile(tempFile, list);

                Assert.Single(list);
                Assert.Equal(Path.GetFileName(tempFile), list[0].Name);
                Assert.Equal(content, list[0].Contents);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void LoadFile_WhenListHasExistingItems_AppendsToSameList()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "content");
                var list = new List<PgnFile> { new PgnFile { Name = "other", Contents = "x" } };

                FileHandlerHelper.LoadFile(tempFile, list);

                Assert.Equal(2, list.Count);
                Assert.Equal("content", list[1].Contents);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void LoadFile_WhenFileDoesNotExist_Throws()
        {
            var list = new List<PgnFile>();
            var badPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pgn");
            Assert.Throws<FileNotFoundException>(() => FileHandlerHelper.LoadFile(badPath, list));
        }
    }
}
