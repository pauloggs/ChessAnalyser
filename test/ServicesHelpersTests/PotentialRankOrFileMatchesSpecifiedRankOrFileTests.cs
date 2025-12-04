namespace ServicesHelpersTests
{
    public class PotentialRankOrFileMatchesSpecifiedRankOrFileTests
    {
        [Fact]
        public void PotentialRankOrFileMatchesSpecifiedRankOrFile_ShouldReturnTrue_WhenNeitherSpecified()
        {
            // Arrange
            int potentialRank = 3;
            int potentialFile = 4;
            int specifiedRank = -1;
            int specifiedFile = -1;

            // Act
            var result = Services.Helpers.RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                potentialRank,
                potentialFile,
                specifiedRank,
                specifiedFile);

            // Assert
            Assert.True(result);
        }
        [Fact]
        public void PotentialRankOrFileMatchesSpecifiedRankOrFile_ShouldReturnTrue_WhenRankMatches()
        {
            // Arrange
            int potentialRank = 5;
            int potentialFile = 2;
            int specifiedRank = 5;
            int specifiedFile = -1;

            // Act
            var result = Services.Helpers.RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                potentialRank,
                potentialFile,
                specifiedRank,
                specifiedFile);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void PotentialRankOrFileMatchesSpecifiedRankOrFile_ShouldReturnTrue_WhenFileMatches()
        {
            // Arrange
            int potentialRank = 1;
            int potentialFile = 6;
            int specifiedRank = -1;
            int specifiedFile = 6;

            // Act
            var result = Services.Helpers.RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                potentialRank,
                potentialFile,
                specifiedRank,
                specifiedFile);

            // Assert
            Assert.True(result);
        }
    }
}
