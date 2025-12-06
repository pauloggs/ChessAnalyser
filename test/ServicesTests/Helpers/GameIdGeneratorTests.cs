using FluentAssertions;

namespace ServicesTests.Helpers
{
    public class GameIdGeneratorTests
    {
        public GameIdGeneratorTests() { }

        [Fact]
        public void GetGameId_ShouldReturnCorrectId()
        {
            // Arrange
            var plies = new Dictionary<int, Interfaces.DTO.Ply>
            {
                { 1, new Interfaces.DTO.Ply { RawMove = "e4" } },
                { 2, new Interfaces.DTO.Ply { RawMove = "e5" } },
                { 3, new Interfaces.DTO.Ply { RawMove = "Nf3" } },
                { 4, new Interfaces.DTO.Ply { RawMove = "Nc6" } }
            };

            // Act
            var gameId = Services.Helpers.GameIdGenerator.GetGameId(plies);

            // Assert
            var expectedGameId = "060a07a58215100f6eae66201314110af4dcb579aeb7a23ff765cbd78c708d8e";
            gameId.Should().Be(expectedGameId);
        }

        [Fact]
        public void GetGameId_ShouldReturnEmptyString_ForNoPlies()
        {             
            // Arrange
            var plies = new Dictionary<int, Interfaces.DTO.Ply>();
            // Act
            var gameId = Services.Helpers.GameIdGenerator.GetGameId(plies);
            // Assert
            var expectedGameId = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
            gameId.Should().Be(expectedGameId);
        }
    }
}
