using Interfaces.Analytics;
using Interfaces.DTO;
using Moq;
using Services;
using Services.Analytics;
using Services.Helpers;

namespace ServicesTests.Analytics;

public class GamePositionSummaryFactoryTests
{
    [Fact]
    public void Create_FromStartingPosition_MatchesStandardCountsAndMaterial()
    {
        var pieceValues = new ClassicalPieceValues();
        var sut = new GamePositionSummaryFactory(pieceValues);
        var helper = new BoardPositionsHelper(
            new Mock<IMoveInterpreter>().Object,
            new Mock<IDisplayService>().Object,
            new Mock<IBoardPositionCalculator>().Object);
        var start = helper.GetStartingBoardPosition();

        var row = sut.Create(gameId: 42, plyIndex: -1, start);

        Assert.Equal(42, row.GameId);
        Assert.Equal(-1, row.PlyIndex);
        Assert.Equal(39, row.WhiteMaterial);
        Assert.Equal(39, row.BlackMaterial);
        Assert.Equal(8, row.WhitePawnCount);
        Assert.Equal(2, row.WhiteKnightCount);
        Assert.Equal(2, row.WhiteBishopCount);
        Assert.Equal(2, row.WhiteRookCount);
        Assert.Equal(1, row.WhiteQueenCount);
        Assert.Equal(1, row.WhiteKingCount);
        Assert.Equal(8, row.BlackPawnCount);
        Assert.Equal(2, row.BlackKnightCount);
        Assert.Equal(2, row.BlackBishopCount);
        Assert.Equal(2, row.BlackRookCount);
        Assert.Equal(1, row.BlackQueenCount);
        Assert.Equal(1, row.BlackKingCount);
    }

    [Fact]
    public void Create_NullBoard_Throws()
    {
        var sut = new GamePositionSummaryFactory(new ClassicalPieceValues());
        Assert.Throws<ArgumentNullException>(() => sut.Create(1, 0, null!));
    }
}
