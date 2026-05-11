using Interfaces.Analytics;
using Moq;
using Repositories;
using Services.Analytics;

namespace ServicesTests.Analytics;

public class MetricRegistryAndExecutorsTests
{
    [Fact]
    public void MetricRegistry_ContainsBothReferenceMetricKeys()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetMaterialAveragesByYearAtPlyAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MaterialAverageByYearRow>());
        repo.Setup(r => r.GetKnightDestinationCountsAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<KnightDestinationCountRow>());
        repo.Setup(r => r.GetGameCountsByEcoAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<GameCountByEcoRow>());

        var sut = new MetricRegistry(new IMetricExecutor[]
        {
            new AverageMaterialByYearAndColourExecutor(repo.Object),
            new KnightMoveDestinationFrequencyExecutor(repo.Object),
            new GameCountByEcoExecutor(repo.Object)
        });

        Assert.Contains("AverageMaterialByYearAndColour", sut.MetricKeys);
        Assert.Contains("KnightMoveDestinationFrequency", sut.MetricKeys);
        Assert.Contains("GameCountByEco", sut.MetricKeys);
        Assert.Equal(3, sut.MetricKeys.Count);
    }

    [Fact]
    public void MetricRegistry_TryGetExecutor_IsCaseInsensitive()
    {
        var repo = new Mock<IChessRepository>();
        var sut = new MetricRegistry(new IMetricExecutor[]
        {
            new AverageMaterialByYearAndColourExecutor(repo.Object),
            new KnightMoveDestinationFrequencyExecutor(repo.Object)
        });

        Assert.NotNull(sut.TryGetExecutor("averageMaterialByYearAndColour"));
        Assert.Null(sut.TryGetExecutor("UnknownMetric"));
    }

    [Fact]
    public async Task MetricRegistry_ExecuteAsync_UnknownKey_ThrowsKeyNotFoundException()
    {
        var repo = new Mock<IChessRepository>();
        var sut = new MetricRegistry(new IMetricExecutor[]
        {
            new AverageMaterialByYearAndColourExecutor(repo.Object)
        });

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            sut.ExecuteAsync("NotARealMetric", new AnalyticsQuery()));
    }

    [Fact]
    public async Task AverageMaterialByYearAndColourExecutor_UsesDefaultPly4_WhenSummaryPlyNull()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetMaterialAveragesByYearAtPlyAsync(It.IsAny<AnalyticsQuery>(), 4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MaterialAverageByYearRow>
            {
                new() { GameYear = 1934, AvgWhiteMaterial = 39.5, AvgBlackMaterial = 39.0, GameCount = 2 }
            });

        var sut = new AverageMaterialByYearAndColourExecutor(repo.Object);
        var result = await sut.ExecuteAsync(new AnalyticsQuery());

        Assert.Equal(["GameYear", "AvgWhiteMaterial", "AvgBlackMaterial", "GameCount"], result.ColumnNames);
        Assert.Single(result.Rows);
        Assert.Equal((short)1934, result.Rows[0][0]);
        Assert.Equal(39.5, result.Rows[0][1]);
        Assert.Equal(39.0, result.Rows[0][2]);
        Assert.Equal(2, result.Rows[0][3]);
    }

    [Fact]
    public async Task AverageMaterialByYearAndColourExecutor_UsesSummaryPlyFromQuery_WhenSet()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetMaterialAveragesByYearAtPlyAsync(It.IsAny<AnalyticsQuery>(), 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MaterialAverageByYearRow>());

        var sut = new AverageMaterialByYearAndColourExecutor(repo.Object);
        await sut.ExecuteAsync(new AnalyticsQuery { SummaryPlyIndex = 10 });

        repo.Verify(r => r.GetMaterialAveragesByYearAtPlyAsync(It.IsAny<AnalyticsQuery>(), 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task KnightMoveDestinationFrequencyExecutor_MapsRows()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetKnightDestinationCountsAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<KnightDestinationCountRow>
            {
                new() { ToSquare = 36, MoveCount = 5 },
                new() { ToSquare = 27, MoveCount = 1 }
            });

        var sut = new KnightMoveDestinationFrequencyExecutor(repo.Object);
        var result = await sut.ExecuteAsync(new AnalyticsQuery());

        Assert.Equal(["ToSquare", "MoveCount"], result.ColumnNames);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal((byte)36, result.Rows[0][0]);
        Assert.Equal(5, result.Rows[0][1]);
    }

    [Fact]
    public async Task KnightMoveDestinationFrequencyExecutor_PassesQueryToRepository()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetKnightDestinationCountsAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<KnightDestinationCountRow>());

        var query = new AnalyticsQuery { MinGameYear = 2000, Eco = "C00", WhitePlayerSurname = "Tal", WhitePlayerForenames = "Mikhail" };
        var sut = new KnightMoveDestinationFrequencyExecutor(repo.Object);
        await sut.ExecuteAsync(query);

        repo.Verify(r => r.GetKnightDestinationCountsAsync(
            It.Is<AnalyticsQuery>(q =>
                q.MinGameYear == 2000
                && q.Eco == "C00"
                && q.WhitePlayerSurname == "Tal"
                && q.WhitePlayerForenames == "Mikhail"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GameCountByEcoExecutor_MapsRows()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetGameCountsByEcoAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameCountByEcoRow>
            {
                new() { Eco = "B90", GameCount = 12 },
                new() { Eco = "C00", GameCount = 5 }
            });

        var sut = new GameCountByEcoExecutor(repo.Object);
        var result = await sut.ExecuteAsync(new AnalyticsQuery());

        Assert.Equal(["Eco", "GameCount"], result.ColumnNames);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal("B90", result.Rows[0][0]);
        Assert.Equal(12, result.Rows[0][1]);
    }

    [Fact]
    public async Task GameCountByEcoExecutor_PassesNameFiltersToRepository()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetGameCountsByEcoAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<GameCountByEcoRow>());

        var query = new AnalyticsQuery
        {
            MinGameYear = 1980,
            WhitePlayerSurname = "Kasparov",
            WhitePlayerForenames = "Garry",
            Eco = "B90"
        };
        var sut = new GameCountByEcoExecutor(repo.Object);

        await sut.ExecuteAsync(query);

        repo.Verify(r => r.GetGameCountsByEcoAsync(
            It.Is<AnalyticsQuery>(q =>
                q.MinGameYear == 1980
                && q.WhitePlayerSurname == "Kasparov"
                && q.WhitePlayerForenames == "Garry"
                && q.Eco == "B90"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
