namespace ServicesHelpersTests
{
    public class TagHelperTests
    {
        [Fact]
        public void AddGameTag_ShouldAddTagToDictionary()
        {
            // Arrange
            var tagDictionary = new Dictionary<string, string>();
            string line = "[Event \"F/S Return Match\"]";
            // Act
            Services.Helpers.TagHelper.AddGameTag(tagDictionary, line);
            // Assert
            Assert.True(tagDictionary.ContainsKey("event"));
            Assert.Equal("F/S Return Match", tagDictionary["event"]);
        }

        [Fact]
        public void AddGameTag_ShouldOverwriteExistingTag()
        {
            // Arrange
            var tagDictionary = new Dictionary<string, string>
            {
                { "event", "Old Event" }
            };
            string line = "[Event \"New Event\"]";
            // Act
            Services.Helpers.TagHelper.AddGameTag(tagDictionary, line);
            // Assert
            Assert.True(tagDictionary.ContainsKey("event"));
            Assert.Equal("New Event", tagDictionary["event"]);
        }

        [Fact]
        public void AddGameTag_ShouldHandleWhitespace()
        {
            // Arrange
            var tagDictionary = new Dictionary<string, string>();
            string line = "   [Site \"Belgrade, Serbia JUG\"]   ";
            // Act
            Services.Helpers.TagHelper.AddGameTag(tagDictionary, line.Trim());
            // Assert
            Assert.True(tagDictionary.ContainsKey("site"));
            Assert.Equal("Belgrade, Serbia JUG", tagDictionary["site"]);
        }

        [Fact]
        public void AddGameTag_ShouldHandleDifferentTagFormats()
        {
            // Arrange
            var tagDictionary = new Dictionary<string, string>();
            string line = "[Date \"1992.11.04\"]";
            // Act
            Services.Helpers.TagHelper.AddGameTag(tagDictionary, line);
            // Assert
            Assert.True(tagDictionary.ContainsKey("date"));
            Assert.Equal("1992.11.04", tagDictionary["date"]);
        }
    }
}
