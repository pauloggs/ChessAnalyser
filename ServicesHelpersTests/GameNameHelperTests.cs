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
    }
}
