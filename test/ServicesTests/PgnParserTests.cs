using Moq;
using Services;

namespace ServicesTests
{
    public class PgnParserTests
    {
        Mock<INaming> namingMock;
        Mock<IBoardPositionService> boardPositionServiceMock;
        IPgnParser sut;

        public PgnParserTests()
        {
            namingMock = new Mock<INaming>();
            boardPositionServiceMock = new Mock<IBoardPositionService>();
            sut = new PgnParser(
                namingMock.Object,
                boardPositionServiceMock.Object);
        }

        public void GetPgnGamesFromPgnFiles_ShouldReturnGames()
        {
            // Arrange


            // Act

            // Assert
        }
    }
}
