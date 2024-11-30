using FluentAssertions;
using Interfaces.DTO;
using Moq;
using Services;

namespace ServicesTests
{
    public class FileSplitterTests
    {
        private readonly IPgnParser _sut;
        private Mock<INaming> mockNaming;
        private Mock<IBoardPositionGenerator> mockBoardPositionGenerator;
        private Mock<IGameIdGenerator> mockGameIdGenerator;

        public FileSplitterTests()
        {
            mockNaming = new Mock<INaming>();
            mockBoardPositionGenerator = new Mock<IBoardPositionGenerator>();
            mockGameIdGenerator = new Mock<IGameIdGenerator>();
            _sut = new PgnParser(mockNaming.Object, mockBoardPositionGenerator.Object, mockGameIdGenerator.Object);
        }

        [Fact]
        public void GetRawGamesFromPgnFile_ShouldReturnEmptyListIfNoEventFound()
        {
            // Arrange
            var testRawPgnWithNoEventInContents = new RawPgn()
            {
                Name = "SomeName.pgn",
                Contents = "SomeContents"
            };

            // Act
            var result = _sut.GetRawGamesFromPgnFile(testRawPgnWithNoEventInContents);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(0);
        }
    }
}
