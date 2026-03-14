namespace ServicesHelpersTests
{
    public class GameNameHelperTests
    {
        [Fact]
        public void GetGameName_ShouldThrowArgumentNullException_WhenTagDictionaryIsNull()
        {
            // Arrange
            Dictionary<string, string>? tagDictionary = null;

            // Act & Assert
            _ = Assert.Throws<ArgumentNullException>(() => Services.Helpers.GameNameHelper.GetGameName(tagDicionary: tagDictionary));
        }

        [Fact]
        public void GetGameName_ShouldThrowArgumentException_WhenTagDictionaryIsEmpty()
        {
            // Arrange
            var tagDictionary = new Dictionary<string, string>();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => Services.Helpers.GameNameHelper.GetGameName(tagDictionary));
        }

        [Fact]
        public void GetGameName_ShouldReturnCorrectGameName_WhenTagDictionaryIsValid()
        {
            // Arrange
            var validTags = Interfaces.Constants.GameTagIdentifiers;

            var tagDictionary = new Dictionary<string, string>
            {
                { validTags[0], "Tag0Value" },
                { validTags[1], "Tag1Value" }
            };
            var expectedGameName = "Tag0Value|Tag1Value";
            // Act
            var actualGameName = Services.Helpers.GameNameHelper.GetGameName(tagDictionary);
            // Assert
            Assert.Equal(expectedGameName, actualGameName);
        }

        [Fact]
        public void GetGameName_WhenTagValueIsQuestionMarkOrEmpty_UsesDash()
        {
            var tagDictionary = new Dictionary<string, string>
            {
                { "event", "?" },
                { "site", "  ?  " },
                { "date", "1904.??.??" },
                { "round", "" },
                { "white", "Giese" },
                { "black", "Alekhine, Alexander A" },
                { "result", "0-1" },
                { "eco", "C33" }
            };
            var actual = Services.Helpers.GameNameHelper.GetGameName(tagDictionary);
            Assert.Equal("-|-|1904.??.??|-|Giese|Alekhine, Alexander A|0-1|C33", actual);
        }
    }
}
