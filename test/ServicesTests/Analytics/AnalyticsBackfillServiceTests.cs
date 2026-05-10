using Interfaces.Analytics;
using Interfaces.DTO;
using Moq;
using Repositories;
using Services.Analytics;

namespace ServicesTests.Analytics;

public class AnalyticsBackfillServiceTests
{
    [Fact]
    public async Task BackfillMissingAnalyticsAsync_RespectsMaxGames()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetGameIdsNeedingAnalyticsBackfillAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 10, 20, 30 });
        repo.Setup(r => r.GetBoardPositionsForGameOrderedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(int PlyIndex, BoardPosition Position)>());

        var mat = new Mock<IAnalyticsMaterializationService>();
        mat.Setup(m => m.MaterializeFromOrderedPliesAsync(It.IsAny<int>(), It.IsAny<IReadOnlyList<(int PlyIndex, BoardPosition Position)>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AnalyticsMaterializationOutcome.SkippedNoPositions);

        var sut = new AnalyticsBackfillService(repo.Object, mat.Object);
        var result = await sut.BackfillMissingAnalyticsAsync(new AnalyticsBackfillOptions { MaxGames = 2 });

        Assert.Equal(2, result.GamesConsidered);
        mat.Verify(m => m.MaterializeFromOrderedPliesAsync(10, It.IsAny<IReadOnlyList<(int PlyIndex, BoardPosition Position)>>(), It.IsAny<CancellationToken>()), Times.Once);
        mat.Verify(m => m.MaterializeFromOrderedPliesAsync(20, It.IsAny<IReadOnlyList<(int PlyIndex, BoardPosition Position)>>(), It.IsAny<CancellationToken>()), Times.Once);
        mat.Verify(m => m.MaterializeFromOrderedPliesAsync(30, It.IsAny<IReadOnlyList<(int PlyIndex, BoardPosition Position)>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BackfillMissingAnalyticsAsync_CountsOutcomes()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetGameIdsNeedingAnalyticsBackfillAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1, 2, 3 });
        repo.Setup(r => r.GetBoardPositionsForGameOrderedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(int PlyIndex, BoardPosition Position)> { (-1, new BoardPosition()) });

        var mat = new Mock<IAnalyticsMaterializationService>();
        mat.SetupSequence(m => m.MaterializeFromOrderedPliesAsync(It.IsAny<int>(), It.IsAny<IReadOnlyList<(int PlyIndex, BoardPosition Position)>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AnalyticsMaterializationOutcome.Success)
            .ReturnsAsync(AnalyticsMaterializationOutcome.SkippedInvalidSequence)
            .ReturnsAsync(AnalyticsMaterializationOutcome.Failed);

        var sut = new AnalyticsBackfillService(repo.Object, mat.Object);
        var result = await sut.BackfillMissingAnalyticsAsync();

        Assert.Equal(3, result.GamesConsidered);
        Assert.Equal(1, result.GamesMaterialized);
        Assert.Equal(1, result.GamesSkipped);
        Assert.Equal(1, result.GamesFailed);
    }
}
