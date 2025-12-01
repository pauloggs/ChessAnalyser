using FluentAssertions;

namespace ServicesHelpersTests
{
    public class GameIdGeneratorTests
    {
        public GameIdGeneratorTests() { }

        [Fact]
        public void CheckAndReturnGameId_ShouldReturnCorrectId()
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
            var gameId = Services.Helpers.GameIdGenerator.CheckAndReturnGameId(plies);

            // Assert
            var expectedGameId = "e4|e5|Nf3|Nc6";
            gameId.Should().Be(expectedGameId);
        }

        [Fact]
        public void CheckAndReturnGameId_ShouldReturnEmptyString_ForNoPlies()
        {             
            // Arrange
            var plies = new Dictionary<int, Interfaces.DTO.Ply>();
            // Act
            var gameId = Services.Helpers.GameIdGenerator.CheckAndReturnGameId(plies);
            // Assert
            var expectedGameId = "";
            gameId.Should().Be(expectedGameId);
        }
    }
}
