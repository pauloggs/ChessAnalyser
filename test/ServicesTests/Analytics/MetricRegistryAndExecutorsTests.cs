using Interfaces.Analytics;
using Moq;
using Repositories;
using Services.Analytics;

namespace ServicesTests.Analytics;

public class MetricRegistryAndExecutorsTests
{
    private static readonly CorpusBenchmarkCalculator CorpusBenchmarkCalculator = new();

    [Fact]
    public void MetricRegistry_ContainsRegisteredMetricKeys()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetMaterialAveragesByYearAtPlyAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MaterialAverageByYearRow>());
        repo.Setup(r => r.GetKnightDestinationCountsAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<KnightDestinationCountRow>());
        repo.Setup(r => r.GetGameCountsByEcoAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<GameCountByEcoRow>());
        repo.Setup(r => r.GetGameCountsByYearAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<GameCountByYearRow>());
        repo.Setup(r => r.GetGameCountsByResultAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<GameCountByResultRow>());
        repo.Setup(r => r.GetGameCountsByPlayerAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<GameCountByPlayerRow>());
        repo.Setup(r => r.GetPlayerResultSummariesAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlayerResultSummaryRow>());
        repo.Setup(r => r.GetPlayerMaterialAveragesAtPlyAsync(
                It.IsAny<AnalyticsQuery>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlayerMaterialAverageRow>());
        repo.Setup(r => r.GetAverageCastlingPlyAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AverageCastlingPlyRow>());
        repo.Setup(r => r.GetPerPlayerAverageCastlingPlyAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlayerStylePerPlayerMetricRow>());
        repo.Setup(r => r.GetAverageMaterialVolatilityAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AverageMaterialVolatilityRow>());
        repo.Setup(r => r.GetPerPlayerAverageMaterialVolatilityAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlayerStylePerPlayerMetricRow>());
        repo.Setup(r => r.GetBishopPairFrequencyAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<BishopPairFrequencyRow>());
        repo.Setup(r => r.GetPerPlayerBishopPairFrequencyAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlayerStylePerPlayerMetricRow>());
        repo.Setup(r => r.GetMinorPieceCompositionAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MinorPieceCompositionRow>());
        repo.Setup(r => r.GetPerPlayerMinorPieceCompositionAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlayerStylePerPlayerMetricRow>());
        repo.Setup(r => r.GetCaptureRateAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CaptureRateRow>());
        repo.Setup(r => r.GetPerPlayerCaptureRateAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlayerStylePerPlayerMetricRow>());
        repo.Setup(r => r.GetQueenTradeRateAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<QueenTradeRateRow>());

        var sut = new MetricRegistry(new IMetricExecutor[]
        {
            new AverageMaterialByYearAndColourExecutor(repo.Object),
            new KnightMoveDestinationFrequencyExecutor(repo.Object),
            new GameCountByEcoExecutor(repo.Object),
            new GameCountByYearExecutor(repo.Object),
            new GameCountByResultExecutor(repo.Object),
            new GameCountByPlayerExecutor(repo.Object),
            new PlayerResultSummaryExecutor(repo.Object),
            new AverageMaterialByPlayerAtMoveExecutor(repo.Object),
            new AverageCastlingPlyExecutor(repo.Object, CorpusBenchmarkCalculator),
            new AverageMaterialVolatilityExecutor(repo.Object, CorpusBenchmarkCalculator),
            new BishopPairFrequencyExecutor(repo.Object, CorpusBenchmarkCalculator),
            new MinorPieceCompositionExecutor(repo.Object, CorpusBenchmarkCalculator),
            new CaptureRateExecutor(repo.Object, CorpusBenchmarkCalculator),
            new QueenTradeRateExecutor(repo.Object)
        });

        Assert.Contains("AverageMaterialByYearAndColour", sut.MetricKeys);
        Assert.Contains("KnightMoveDestinationFrequency", sut.MetricKeys);
        Assert.Contains("GameCountByEco", sut.MetricKeys);
        Assert.Contains("GameCountByYear", sut.MetricKeys);
        Assert.Contains("GameCountByResult", sut.MetricKeys);
        Assert.Contains("GameCountByPlayer", sut.MetricKeys);
        Assert.Contains("PlayerResultSummary", sut.MetricKeys);
        Assert.Contains("AverageMaterialByPlayerAtMove", sut.MetricKeys);
        Assert.Contains("AverageCastlingPly", sut.MetricKeys);
        Assert.Contains("AverageMaterialVolatility", sut.MetricKeys);
        Assert.Contains("BishopPairFrequency", sut.MetricKeys);
        Assert.Contains("MinorPieceComposition", sut.MetricKeys);
        Assert.Contains("CaptureRate", sut.MetricKeys);
        Assert.Contains("QueenTradeRate", sut.MetricKeys);
        Assert.Equal(14, sut.MetricKeys.Count);
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

        var query = new AnalyticsQuery { MinGameYear = 2000, Eco = "C00", PlayerSurname = "Tal", PlayerForenames = "Mikhail", PlayerColour = "Any" };
        var sut = new KnightMoveDestinationFrequencyExecutor(repo.Object);
        await sut.ExecuteAsync(query);

        repo.Verify(r => r.GetKnightDestinationCountsAsync(
            It.Is<AnalyticsQuery>(q =>
                q.MinGameYear == 2000
                && q.Eco == "C00"
                && q.PlayerSurname == "Tal"
                && q.PlayerForenames == "Mikhail"
                && q.PlayerColour == "Any"),
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
            PlayerSurname = "Kasparov",
            PlayerForenames = "Garry",
            PlayerColour = "White",
            Eco = "B90"
        };
        var sut = new GameCountByEcoExecutor(repo.Object);

        await sut.ExecuteAsync(query);

        repo.Verify(r => r.GetGameCountsByEcoAsync(
            It.Is<AnalyticsQuery>(q =>
                q.MinGameYear == 1980
                && q.PlayerSurname == "Kasparov"
                && q.PlayerForenames == "Garry"
                && q.PlayerColour == "White"
                && q.Eco == "B90"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GameCountByYearExecutor_MapsRows()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetGameCountsByYearAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameCountByYearRow>
            {
                new() { GameYear = 1985, GameCount = 12 },
                new() { GameYear = 1986, GameCount = 5 }
            });

        var sut = new GameCountByYearExecutor(repo.Object);
        var result = await sut.ExecuteAsync(new AnalyticsQuery());

        Assert.Equal(["GameYear", "GameCount"], result.ColumnNames);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal((short)1985, result.Rows[0][0]);
        Assert.Equal(12, result.Rows[0][1]);
    }

    [Fact]
    public async Task GameCountByYearExecutor_PassesNameFiltersToRepository()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetGameCountsByYearAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<GameCountByYearRow>());

        var query = new AnalyticsQuery
        {
            MinGameYear = 1980,
            MaxGameYear = 1990,
            PlayerSurname = "Karpov",
            PlayerForenames = "Anatoly",
            PlayerColour = "Black",
            Eco = "B90"
        };
        var sut = new GameCountByYearExecutor(repo.Object);

        await sut.ExecuteAsync(query);

        repo.Verify(r => r.GetGameCountsByYearAsync(
            It.Is<AnalyticsQuery>(q =>
                q.MinGameYear == 1980
                && q.MaxGameYear == 1990
                && q.PlayerSurname == "Karpov"
                && q.PlayerForenames == "Anatoly"
                && q.PlayerColour == "Black"
                && q.Eco == "B90"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GameCountByResultExecutor_MapsRows()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetGameCountsByResultAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameCountByResultRow>
            {
                new() { Result = "White", GameCount = 12 },
                new() { Result = "Draw", GameCount = 5 }
            });

        var sut = new GameCountByResultExecutor(repo.Object);
        var result = await sut.ExecuteAsync(new AnalyticsQuery());

        Assert.Equal(["Result", "GameCount"], result.ColumnNames);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal("White", result.Rows[0][0]);
        Assert.Equal(12, result.Rows[0][1]);
    }

    [Fact]
    public async Task GameCountByResultExecutor_PassesNameFiltersToRepository()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetGameCountsByResultAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<GameCountByResultRow>());

        var query = new AnalyticsQuery
        {
            MinGameYear = 1980,
            MaxGameYear = 1990,
            PlayerSurname = "Kasparov",
            PlayerForenames = "Garry",
            PlayerColour = "Any",
            Eco = "B90"
        };
        var sut = new GameCountByResultExecutor(repo.Object);

        await sut.ExecuteAsync(query);

        repo.Verify(r => r.GetGameCountsByResultAsync(
            It.Is<AnalyticsQuery>(q =>
                q.MinGameYear == 1980
                && q.MaxGameYear == 1990
                && q.PlayerSurname == "Kasparov"
                && q.PlayerForenames == "Garry"
                && q.PlayerColour == "Any"
                && q.Eco == "B90"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GameCountByPlayerExecutor_MapsRows()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetGameCountsByPlayerAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameCountByPlayerRow>
            {
                new() { PlayerSurname = "Kasparov", PlayerForenames = "Garry", WhiteGameCount = 12, BlackGameCount = 8, TotalGameCount = 20 },
                new() { PlayerSurname = "Tal", PlayerForenames = "", WhiteGameCount = 3, BlackGameCount = 2, TotalGameCount = 5 }
            });

        var sut = new GameCountByPlayerExecutor(repo.Object);
        var result = await sut.ExecuteAsync(new AnalyticsQuery());

        Assert.Equal(["Player", "WhiteGameCount", "BlackGameCount", "TotalGameCount"], result.ColumnNames);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal("Kasparov, Garry", result.Rows[0][0]);
        Assert.Equal(12, result.Rows[0][1]);
        Assert.Equal(8, result.Rows[0][2]);
        Assert.Equal(20, result.Rows[0][3]);
        Assert.Equal("Tal", result.Rows[1][0]);
    }

    [Fact]
    public async Task GameCountByPlayerExecutor_PassesNameFiltersToRepository()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetGameCountsByPlayerAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<GameCountByPlayerRow>());

        var query = new AnalyticsQuery
        {
            MinGameYear = 1980,
            MaxGameYear = 1990,
            PlayerSurname = "Kasparov",
            PlayerForenames = "Garry",
            PlayerColour = "White",
            Eco = "B90"
        };
        var sut = new GameCountByPlayerExecutor(repo.Object);

        await sut.ExecuteAsync(query);

        repo.Verify(r => r.GetGameCountsByPlayerAsync(
            It.Is<AnalyticsQuery>(q =>
                q.MinGameYear == 1980
                && q.MaxGameYear == 1990
                && q.PlayerSurname == "Kasparov"
                && q.PlayerForenames == "Garry"
                && q.PlayerColour == "White"
                && q.Eco == "B90"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PlayerResultSummaryExecutor_MapsRows()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetPlayerResultSummariesAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerResultSummaryRow>
            {
                new() { PlayerSurname = "Kasparov", PlayerForenames = "Garry", WinCount = 12, LossCount = 3, DrawCount = 10, UnknownCount = 1, TotalGameCount = 26, Score = 17 },
                new() { PlayerSurname = "Tal", PlayerForenames = "", WinCount = 5, LossCount = 2, DrawCount = 4, UnknownCount = 0, TotalGameCount = 11, Score = 7 }
            });

        var sut = new PlayerResultSummaryExecutor(repo.Object);
        var result = await sut.ExecuteAsync(new AnalyticsQuery());

        Assert.Equal(["Player", "WinCount", "LossCount", "DrawCount", "UnknownCount", "TotalGameCount", "Score"], result.ColumnNames);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal("Kasparov, Garry", result.Rows[0][0]);
        Assert.Equal(12, result.Rows[0][1]);
        Assert.Equal(3, result.Rows[0][2]);
        Assert.Equal(10, result.Rows[0][3]);
        Assert.Equal(1, result.Rows[0][4]);
        Assert.Equal(26, result.Rows[0][5]);
        Assert.Equal(17d, result.Rows[0][6]);
        Assert.Equal("Tal", result.Rows[1][0]);
    }

    [Fact]
    public async Task PlayerResultSummaryExecutor_PassesNameFiltersToRepository()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetPlayerResultSummariesAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlayerResultSummaryRow>());

        var query = new AnalyticsQuery
        {
            MinGameYear = 1980,
            MaxGameYear = 1990,
            PlayerSurname = "Kasparov",
            PlayerForenames = "Garry",
            PlayerColour = "White",
            Eco = "B90"
        };
        var sut = new PlayerResultSummaryExecutor(repo.Object);

        await sut.ExecuteAsync(query);

        repo.Verify(r => r.GetPlayerResultSummariesAsync(
            It.Is<AnalyticsQuery>(q =>
                q.MinGameYear == 1980
                && q.MaxGameYear == 1990
                && q.PlayerSurname == "Kasparov"
                && q.PlayerForenames == "Garry"
                && q.PlayerColour == "White"
                && q.Eco == "B90"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 3)]
    [InlineData(10, 19)]
    public void AverageMaterialByPlayerAtMoveExecutor_MapsMoveNumberToFullMovePly(int moveNumber, int expectedPly)
    {
        Assert.Equal(expectedPly, AverageMaterialByPlayerAtMoveExecutor.MoveNumberToPlyIndex(moveNumber));
    }

    [Fact]
    public void AverageMaterialByPlayerAtMoveExecutor_RejectsMoveNumberLessThanOne()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AverageMaterialByPlayerAtMoveExecutor.MoveNumberToPlyIndex(0));
    }

    [Theory]
    [InlineData(null, "Any")]
    [InlineData("any", "Any")]
    [InlineData("WHITE", "White")]
    [InlineData("black", "Black")]
    public void AverageMaterialByPlayerAtMoveExecutor_NormalizesColourMode(string? input, string expected)
    {
        Assert.Equal(expected, AverageMaterialByPlayerAtMoveExecutor.NormalizeColourMode(input));
    }

    [Fact]
    public async Task AverageMaterialByPlayerAtMoveExecutor_PlayerAComparedWithAllPlayers_MapsRows()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetPlayerMaterialAveragesAtPlyAsync(
                It.IsAny<AnalyticsQuery>(),
                2,
                3,
                "Any",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerMaterialAverageRow>
            {
                new() { Series = "PlayerA", PlayerSurname = "Kasparov", PlayerForenames = "Garry", Colour = "Any", MoveNumber = 2, PlyIndex = 3, AvgMaterial = 38.5, PositionCount = 10 },
                new() { Series = "AllPlayers", PlayerSurname = null, PlayerForenames = null, Colour = "Any", MoveNumber = 2, PlyIndex = 3, AvgMaterial = 39.1, PositionCount = 200 }
            });

        var sut = new AverageMaterialByPlayerAtMoveExecutor(repo.Object);
        var result = await sut.ExecuteAsync(new AnalyticsQuery
        {
            PlayerASurname = "Kasparov",
            PlayerAForenames = "Garry",
            MoveNumber = 2
        });

        Assert.Equal(["Series", "Player", "Colour", "MoveNumber", "PlyIndex", "AvgMaterial", "PositionCount"], result.ColumnNames);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal("PlayerA", result.Rows[0][0]);
        Assert.Equal("Kasparov, Garry", result.Rows[0][1]);
        Assert.Equal(38.5, result.Rows[0][5]);
        Assert.Equal("All players", result.Rows[1][1]);
    }

    [Fact]
    public async Task AverageMaterialByPlayerAtMoveExecutor_PlayerAComparedWithPlayerB_PassesQueryToRepository()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetPlayerMaterialAveragesAtPlyAsync(
                It.IsAny<AnalyticsQuery>(),
                3,
                5,
                "White",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerMaterialAverageRow>
            {
                new() { Series = "PlayerA", PlayerSurname = "Kasparov", PlayerForenames = "Garry", Colour = "White", MoveNumber = 3, PlyIndex = 5, AvgMaterial = 38, PositionCount = 1 },
                new() { Series = "PlayerB", PlayerSurname = "Karpov", PlayerForenames = "Anatoly", Colour = "White", MoveNumber = 3, PlyIndex = 5, AvgMaterial = 39, PositionCount = 1 }
            });

        var query = new AnalyticsQuery
        {
            PlayerASurname = "Kasparov",
            PlayerAForenames = "Garry",
            PlayerBSurname = "Karpov",
            PlayerBForenames = "Anatoly",
            PlayerColour = "White",
            MoveNumber = 3,
            MinGameYear = 1980,
            Eco = "B90"
        };
        var sut = new AverageMaterialByPlayerAtMoveExecutor(repo.Object);

        await sut.ExecuteAsync(query);

        repo.Verify(r => r.GetPlayerMaterialAveragesAtPlyAsync(
            It.Is<AnalyticsQuery>(q =>
                q.PlayerASurname == "Kasparov"
                && q.PlayerAForenames == "Garry"
                && q.PlayerBSurname == "Karpov"
                && q.PlayerBForenames == "Anatoly"
                && q.MinGameYear == 1980
                && q.Eco == "B90"),
            3,
            5,
            "White",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AverageMaterialByPlayerAtMoveExecutor_ReturnsEmpty_WhenPlayerAHasNoRows()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetPlayerMaterialAveragesAtPlyAsync(
                It.IsAny<AnalyticsQuery>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerMaterialAverageRow>
            {
                new() { Series = "AllPlayers", Colour = "Any", MoveNumber = 1, PlyIndex = 1, AvgMaterial = 39, PositionCount = 10 }
            });

        var sut = new AverageMaterialByPlayerAtMoveExecutor(repo.Object);
        var result = await sut.ExecuteAsync(new AnalyticsQuery { PlayerASurname = "NoSuchPlayer" });

        Assert.Empty(result.Rows);
    }

    [Fact]
    public async Task AverageMaterialByPlayerAtMoveExecutor_RequiresPlayerASurname()
    {
        var repo = new Mock<IChessRepository>();
        var sut = new AverageMaterialByPlayerAtMoveExecutor(repo.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.ExecuteAsync(new AnalyticsQuery()));
    }

    [Fact]
    public async Task AverageCastlingPlyExecutor_MapsRow()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetAverageCastlingPlyAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AverageCastlingPlyRow>
            {
                new()
                {
                    PlayerSurname = "Petrosian",
                    PlayerForenames = "Tigran",
                    GamesWithCastling = 2,
                    AverageCastlingPly = 8.0
                }
            });

        var sut = new AverageCastlingPlyExecutor(repo.Object, CorpusBenchmarkCalculator);
        var result = await sut.ExecuteAsync(new AnalyticsQuery
        {
            PlayerSurname = "Petrosian",
            PlayerForenames = "Tigran"
        });

        Assert.Equal(["Player", "GamesWithCastling", "AverageCastlingPly"], result.ColumnNames);
        Assert.Single(result.Rows);
        Assert.Equal("Petrosian, Tigran", result.Rows[0][0]);
        Assert.Equal(2, result.Rows[0][1]);
        Assert.Equal(8.0, result.Rows[0][2]);
    }

    [Fact]
    public async Task AverageCastlingPlyExecutor_AppendsBenchmarkColumns_WhenRequested()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetAverageCastlingPlyAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AverageCastlingPlyRow>
            {
                new()
                {
                    PlayerSurname = "Petrosian",
                    PlayerForenames = "Tigran",
                    GamesWithCastling = 100,
                    AverageCastlingPly = 12.0
                }
            });
        repo.Setup(r => r.GetPerPlayerAverageCastlingPlyAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerStylePerPlayerMetricRow>
            {
                new() { PlayerSurname = "Petrosian", PlayerForenames = "Tigran", GameCount = 100, MetricValue = 12.0 },
                new() { PlayerSurname = "Tal", PlayerForenames = "Mikhail", GameCount = 80, MetricValue = 8.0 },
                new() { PlayerSurname = "Fischer", PlayerForenames = "Robert James", GameCount = 90, MetricValue = 10.0 }
            });

        var sut = new AverageCastlingPlyExecutor(repo.Object, CorpusBenchmarkCalculator);
        var result = await sut.ExecuteAsync(new AnalyticsQuery
        {
            PlayerSurname = "Petrosian",
            PlayerForenames = "Tigran",
            IncludeCorpusBenchmark = true,
            BenchmarkMinGames = 30
        });

        Assert.Equal(
            [
                "Player", "GamesWithCastling", "AverageCastlingPly",
                "CorpusAverage", "DeltaFromCorpus", "CorpusPercentile", "CorpusEligiblePlayerCount"
            ],
            result.ColumnNames);
        Assert.Equal(12.0, result.Rows[0][2]);
        Assert.Equal(9.0, result.Rows[0][3]);
        Assert.Equal(3.0, (double)result.Rows[0][4]!, precision: 10);
        Assert.Equal(100.0, result.Rows[0][5]);
        Assert.Equal(2, result.Rows[0][6]);
    }

    [Fact]
    public async Task AverageCastlingPlyExecutor_RequiresPlayerSurname()
    {
        var repo = new Mock<IChessRepository>();
        var sut = new AverageCastlingPlyExecutor(repo.Object, CorpusBenchmarkCalculator);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.ExecuteAsync(new AnalyticsQuery()));
    }

    [Fact]
    public async Task AverageCastlingPlyExecutor_PassesFiltersToRepository()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetAverageCastlingPlyAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AverageCastlingPlyRow>());

        var query = new AnalyticsQuery
        {
            PlayerSurname = "Tal",
            PlayerForenames = "Mikhail",
            PlayerColour = "White",
            MinGameYear = 1960,
            MaxGameYear = 1970,
            Eco = "B90"
        };
        var sut = new AverageCastlingPlyExecutor(repo.Object, CorpusBenchmarkCalculator);

        await sut.ExecuteAsync(query);

        repo.Verify(r => r.GetAverageCastlingPlyAsync(
            It.Is<AnalyticsQuery>(q =>
                q.PlayerSurname == "Tal"
                && q.PlayerForenames == "Mikhail"
                && q.PlayerColour == "White"
                && q.MinGameYear == 1960
                && q.MaxGameYear == 1970
                && q.Eco == "B90"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AverageMaterialVolatilityExecutor_MapsRow()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetAverageMaterialVolatilityAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AverageMaterialVolatilityRow>
            {
                new()
                {
                    PlayerSurname = "Tal",
                    PlayerForenames = "Mikhail",
                    GameCount = 3,
                    AverageMaterialVolatility = 2.5
                }
            });

        var sut = new AverageMaterialVolatilityExecutor(repo.Object, CorpusBenchmarkCalculator);
        var result = await sut.ExecuteAsync(new AnalyticsQuery
        {
            PlayerSurname = "Tal",
            PlayerForenames = "Mikhail"
        });

        Assert.Equal(["Player", "GameCount", "AverageMaterialVolatility"], result.ColumnNames);
        Assert.Single(result.Rows);
        Assert.Equal("Tal, Mikhail", result.Rows[0][0]);
        Assert.Equal(3, result.Rows[0][1]);
        Assert.Equal(2.5, result.Rows[0][2]);
    }

    [Fact]
    public async Task AverageMaterialVolatilityExecutor_RequiresPlayerSurname()
    {
        var repo = new Mock<IChessRepository>();
        var sut = new AverageMaterialVolatilityExecutor(repo.Object, CorpusBenchmarkCalculator);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.ExecuteAsync(new AnalyticsQuery()));
    }

    [Fact]
    public async Task AverageMaterialVolatilityExecutor_PassesPlyWindowToRepository()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetAverageMaterialVolatilityAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AverageMaterialVolatilityRow>());
        repo.Setup(r => r.GetPerPlayerAverageMaterialVolatilityAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlayerStylePerPlayerMetricRow>());

        var query = new AnalyticsQuery
        {
            PlayerSurname = "Karpov",
            PlayerForenames = "Anatoly",
            MinPlyIndex = 15,
            MaxPlyIndex = 40
        };
        var sut = new AverageMaterialVolatilityExecutor(repo.Object, CorpusBenchmarkCalculator);

        await sut.ExecuteAsync(query);

        repo.Verify(r => r.GetAverageMaterialVolatilityAsync(
            It.Is<AnalyticsQuery>(q => q.MinPlyIndex == 15 && q.MaxPlyIndex == 40),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AverageMaterialVolatilityExecutor_AppendsBenchmarkColumns_WhenRequested()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetAverageMaterialVolatilityAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AverageMaterialVolatilityRow>
            {
                new()
                {
                    PlayerSurname = "Fischer",
                    PlayerForenames = "Robert James",
                    GameCount = 827,
                    AverageMaterialVolatility = 1.34
                }
            });
        repo.Setup(r => r.GetPerPlayerAverageMaterialVolatilityAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerStylePerPlayerMetricRow>
            {
                new() { PlayerSurname = "Fischer", PlayerForenames = "Robert James", GameCount = 827, MetricValue = 1.34 },
                new() { PlayerSurname = "Petrosian", PlayerForenames = "Tigran", GameCount = 400, MetricValue = 1.0 },
                new() { PlayerSurname = "Tal", PlayerForenames = "Mikhail", GameCount = 350, MetricValue = 1.8 }
            });

        var sut = new AverageMaterialVolatilityExecutor(repo.Object, CorpusBenchmarkCalculator);
        var result = await sut.ExecuteAsync(new AnalyticsQuery
        {
            PlayerSurname = "Fischer",
            PlayerForenames = "Robert James",
            IncludeCorpusBenchmark = true,
            BenchmarkMinGames = 30
        });

        Assert.Equal(
            ["Player", "GameCount", "AverageMaterialVolatility", "CorpusAverage", "DeltaFromCorpus", "CorpusPercentile", "CorpusEligiblePlayerCount"],
            result.ColumnNames);
        Assert.Equal(1.34, result.Rows[0][2]);
        Assert.Equal(1.4, result.Rows[0][3]);
        Assert.Equal(-0.06, (double)result.Rows[0][4]!, precision: 10);
        Assert.Equal(50.0, result.Rows[0][5]);
        Assert.Equal(2, result.Rows[0][6]);
    }

    [Fact]
    public async Task BishopPairFrequencyExecutor_MapsRowAndUsesDefaultPlyWindow()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetBishopPairFrequencyAsync(
                It.IsAny<AnalyticsQuery>(),
                15,
                30,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BishopPairFrequencyRow>
            {
                new()
                {
                    PlayerSurname = "Karpov",
                    PlayerForenames = "Anatoly",
                    GameCount = 5,
                    AverageBishopPairFrequency = 0.42,
                    MinPlyIndex = 15,
                    MaxPlyIndex = 30
                }
            });

        var sut = new BishopPairFrequencyExecutor(repo.Object, CorpusBenchmarkCalculator);
        var result = await sut.ExecuteAsync(new AnalyticsQuery
        {
            PlayerSurname = "Karpov",
            PlayerForenames = "Anatoly"
        });

        Assert.Equal(["Player", "GameCount", "AverageBishopPairFrequency", "MinPlyIndex", "MaxPlyIndex"], result.ColumnNames);
        Assert.Single(result.Rows);
        Assert.Equal("Karpov, Anatoly", result.Rows[0][0]);
        Assert.Equal(0.42, result.Rows[0][2]);
    }

    [Fact]
    public async Task BishopPairFrequencyExecutor_AppendsBenchmarkColumns_WhenRequested()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetBishopPairFrequencyAsync(
                It.IsAny<AnalyticsQuery>(),
                15,
                30,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BishopPairFrequencyRow>
            {
                new()
                {
                    PlayerSurname = "Karpov",
                    PlayerForenames = "Anatoly",
                    GameCount = 120,
                    AverageBishopPairFrequency = 0.55,
                    MinPlyIndex = 15,
                    MaxPlyIndex = 30
                }
            });
        repo.Setup(r => r.GetPerPlayerBishopPairFrequencyAsync(
                It.IsAny<AnalyticsQuery>(),
                15,
                30,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerStylePerPlayerMetricRow>
            {
                new() { PlayerSurname = "Karpov", PlayerForenames = "Anatoly", GameCount = 120, MetricValue = 0.55 },
                new() { PlayerSurname = "Petrosian", PlayerForenames = "Tigran", GameCount = 80, MetricValue = 0.35 },
                new() { PlayerSurname = "Tal", PlayerForenames = "Mikhail", GameCount = 70, MetricValue = 0.45 }
            });

        var sut = new BishopPairFrequencyExecutor(repo.Object, CorpusBenchmarkCalculator);
        var result = await sut.ExecuteAsync(new AnalyticsQuery
        {
            PlayerSurname = "Karpov",
            PlayerForenames = "Anatoly",
            IncludeCorpusBenchmark = true,
            BenchmarkMinGames = 30
        });

        Assert.Equal(
            [
                "Player", "GameCount", "AverageBishopPairFrequency", "MinPlyIndex", "MaxPlyIndex",
                "CorpusAverage", "DeltaFromCorpus", "CorpusPercentile", "CorpusEligiblePlayerCount"
            ],
            result.ColumnNames);
        Assert.Equal(0.55, result.Rows[0][2]);
        Assert.Equal(0.4, result.Rows[0][5]);
        Assert.Equal(0.15, (double)result.Rows[0][6]!, precision: 10);
        Assert.Equal(100.0, result.Rows[0][7]);
        Assert.Equal(2, result.Rows[0][8]);
    }

    [Fact]
    public async Task BishopPairFrequencyExecutor_RequiresPlayerSurname()
    {
        var repo = new Mock<IChessRepository>();
        var sut = new BishopPairFrequencyExecutor(repo.Object, CorpusBenchmarkCalculator);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.ExecuteAsync(new AnalyticsQuery()));
    }

    [Fact]
    public async Task MinorPieceCompositionExecutor_PassesCustomPlyWindow()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetMinorPieceCompositionAsync(
                It.IsAny<AnalyticsQuery>(),
                20,
                35,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MinorPieceCompositionRow>
            {
                new()
                {
                    PlayerSurname = "Tal",
                    PlayerForenames = "Mikhail",
                    GameCount = 3,
                    AverageMinorPieceDelta = -0.25,
                    MinPlyIndex = 20,
                    MaxPlyIndex = 35
                }
            });

        var sut = new MinorPieceCompositionExecutor(repo.Object, CorpusBenchmarkCalculator);
        var result = await sut.ExecuteAsync(new AnalyticsQuery
        {
            PlayerSurname = "Tal",
            PlayerForenames = "Mikhail",
            MinPlyIndex = 20,
            MaxPlyIndex = 35
        });

        Assert.Single(result.Rows);
        Assert.Equal(-0.25, result.Rows[0][2]);
        repo.Verify(r => r.GetMinorPieceCompositionAsync(
            It.IsAny<AnalyticsQuery>(),
            20,
            35,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MinorPieceCompositionExecutor_AppendsBenchmarkColumns_WhenRequested()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetMinorPieceCompositionAsync(
                It.IsAny<AnalyticsQuery>(),
                15,
                30,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MinorPieceCompositionRow>
            {
                new()
                {
                    PlayerSurname = "Tal",
                    PlayerForenames = "Mikhail",
                    GameCount = 90,
                    AverageMinorPieceDelta = 0.6,
                    MinPlyIndex = 15,
                    MaxPlyIndex = 30
                }
            });
        repo.Setup(r => r.GetPerPlayerMinorPieceCompositionAsync(
                It.IsAny<AnalyticsQuery>(),
                15,
                30,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerStylePerPlayerMetricRow>
            {
                new() { PlayerSurname = "Tal", PlayerForenames = "Mikhail", GameCount = 90, MetricValue = 0.6 },
                new() { PlayerSurname = "Petrosian", PlayerForenames = "Tigran", GameCount = 85, MetricValue = 0.2 },
                new() { PlayerSurname = "Karpov", PlayerForenames = "Anatoly", GameCount = 75, MetricValue = 0.4 }
            });

        var sut = new MinorPieceCompositionExecutor(repo.Object, CorpusBenchmarkCalculator);
        var result = await sut.ExecuteAsync(new AnalyticsQuery
        {
            PlayerSurname = "Tal",
            PlayerForenames = "Mikhail",
            IncludeCorpusBenchmark = true,
            BenchmarkMinGames = 30
        });

        Assert.Equal(
            [
                "Player", "GameCount", "AverageMinorPieceDelta", "MinPlyIndex", "MaxPlyIndex",
                "CorpusAverage", "DeltaFromCorpus", "CorpusPercentile", "CorpusEligiblePlayerCount"
            ],
            result.ColumnNames);
        Assert.Equal(0.6, result.Rows[0][2]);
        Assert.Equal(0.3, (double)result.Rows[0][5]!, precision: 10);
        Assert.Equal(0.3, (double)result.Rows[0][6]!, precision: 10);
        Assert.Equal(100.0, result.Rows[0][7]);
        Assert.Equal(2, result.Rows[0][8]);
    }

    [Fact]
    public async Task MinorPieceCompositionExecutor_RequiresPlayerSurname()
    {
        var repo = new Mock<IChessRepository>();
        var sut = new MinorPieceCompositionExecutor(repo.Object, CorpusBenchmarkCalculator);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.ExecuteAsync(new AnalyticsQuery()));
    }

    [Fact]
    public async Task CaptureRateExecutor_MapsRow()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetCaptureRateAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CaptureRateRow>
            {
                new()
                {
                    PlayerSurname = "Tal",
                    PlayerForenames = "Mikhail",
                    GameCount = 4,
                    AverageCaptureRate = 0.35
                }
            });

        var sut = new CaptureRateExecutor(repo.Object, CorpusBenchmarkCalculator);
        var result = await sut.ExecuteAsync(new AnalyticsQuery
        {
            PlayerSurname = "Tal",
            PlayerForenames = "Mikhail"
        });

        Assert.Equal(["Player", "GameCount", "AverageCaptureRate"], result.ColumnNames);
        Assert.Single(result.Rows);
        Assert.Equal("Tal, Mikhail", result.Rows[0][0]);
        Assert.Equal(4, result.Rows[0][1]);
        Assert.Equal(0.35, result.Rows[0][2]);
    }

    [Fact]
    public async Task CaptureRateExecutor_AppendsBenchmarkColumns_WhenRequested()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetCaptureRateAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CaptureRateRow>
            {
                new()
                {
                    PlayerSurname = "Tal",
                    PlayerForenames = "Mikhail",
                    GameCount = 80,
                    AverageCaptureRate = 0.40
                }
            });
        repo.Setup(r => r.GetPerPlayerCaptureRateAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerStylePerPlayerMetricRow>
            {
                new() { PlayerSurname = "Tal", PlayerForenames = "Mikhail", GameCount = 80, MetricValue = 0.40 },
                new() { PlayerSurname = "Petrosian", PlayerForenames = "Tigran", GameCount = 90, MetricValue = 0.25 },
                new() { PlayerSurname = "Fischer", PlayerForenames = "Robert James", GameCount = 70, MetricValue = 0.35 }
            });

        var sut = new CaptureRateExecutor(repo.Object, CorpusBenchmarkCalculator);
        var result = await sut.ExecuteAsync(new AnalyticsQuery
        {
            PlayerSurname = "Tal",
            PlayerForenames = "Mikhail",
            IncludeCorpusBenchmark = true,
            BenchmarkMinGames = 30
        });

        Assert.Equal(
            [
                "Player", "GameCount", "AverageCaptureRate",
                "CorpusAverage", "DeltaFromCorpus", "CorpusPercentile", "CorpusEligiblePlayerCount"
            ],
            result.ColumnNames);
        Assert.Equal(0.40, result.Rows[0][2]);
        Assert.Equal(0.30, result.Rows[0][3]);
        Assert.Equal(0.10, (double)result.Rows[0][4]!, precision: 10);
        Assert.Equal(100.0, result.Rows[0][5]);
        Assert.Equal(2, result.Rows[0][6]);
    }

    [Fact]
    public async Task CaptureRateExecutor_RequiresPlayerSurname()
    {
        var repo = new Mock<IChessRepository>();
        var sut = new CaptureRateExecutor(repo.Object, CorpusBenchmarkCalculator);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.ExecuteAsync(new AnalyticsQuery()));
    }

    [Fact]
    public async Task CaptureRateExecutor_PassesPlyWindowToRepository()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetCaptureRateAsync(It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CaptureRateRow>());

        var query = new AnalyticsQuery
        {
            PlayerSurname = "Fischer",
            PlayerForenames = "Robert James",
            MinPlyIndex = 10,
            MaxPlyIndex = 50
        };
        var sut = new CaptureRateExecutor(repo.Object, CorpusBenchmarkCalculator);

        await sut.ExecuteAsync(query);

        repo.Verify(r => r.GetCaptureRateAsync(
            It.Is<AnalyticsQuery>(q => q.MinPlyIndex == 10 && q.MaxPlyIndex == 50),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task QueenTradeRateExecutor_MapsRowAndUsesDefaultMaxPly()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetQueenTradeRateAsync(
                It.IsAny<AnalyticsQuery>(),
                40,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<QueenTradeRateRow>
            {
                new()
                {
                    PlayerSurname = "Petrosian",
                    PlayerForenames = "Tigran",
                    GameCount = 6,
                    QueenTradeRate = 0.5,
                    QueenTradeMaxPly = 40
                }
            });

        var sut = new QueenTradeRateExecutor(repo.Object);
        var result = await sut.ExecuteAsync(new AnalyticsQuery
        {
            PlayerSurname = "Petrosian",
            PlayerForenames = "Tigran"
        });

        Assert.Equal(["Player", "GameCount", "QueenTradeRate", "QueenTradeMaxPly"], result.ColumnNames);
        Assert.Single(result.Rows);
        Assert.Equal("Petrosian, Tigran", result.Rows[0][0]);
        Assert.Equal(0.5, result.Rows[0][2]);
        Assert.Equal(40, result.Rows[0][3]);
    }

    [Fact]
    public async Task QueenTradeRateExecutor_RequiresPlayerSurname()
    {
        var repo = new Mock<IChessRepository>();
        var sut = new QueenTradeRateExecutor(repo.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.ExecuteAsync(new AnalyticsQuery()));
    }

    [Fact]
    public async Task QueenTradeRateExecutor_PassesCustomMaxPly()
    {
        var repo = new Mock<IChessRepository>();
        repo.Setup(r => r.GetQueenTradeRateAsync(
                It.IsAny<AnalyticsQuery>(),
                30,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<QueenTradeRateRow>
            {
                new()
                {
                    PlayerSurname = "Karpov",
                    GameCount = 2,
                    QueenTradeRate = 0.0,
                    QueenTradeMaxPly = 30
                }
            });

        var sut = new QueenTradeRateExecutor(repo.Object);
        await sut.ExecuteAsync(new AnalyticsQuery
        {
            PlayerSurname = "Karpov",
            QueenTradeMaxPly = 30
        });

        repo.Verify(r => r.GetQueenTradeRateAsync(
            It.IsAny<AnalyticsQuery>(),
            30,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
