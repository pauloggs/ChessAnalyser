using Interfaces.Analytics;
using Moq;
using Repositories;
using Services.Analytics;

namespace ServicesTests.Analytics;

public class MetricRegistryAndExecutorsTests
{
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
        repo.Setup(r => r.GetPlayerMaterialAveragesAtPlyAsync(
                It.IsAny<AnalyticsQuery>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlayerMaterialAverageRow>());

        var sut = new MetricRegistry(new IMetricExecutor[]
        {
            new AverageMaterialByYearAndColourExecutor(repo.Object),
            new KnightMoveDestinationFrequencyExecutor(repo.Object),
            new GameCountByEcoExecutor(repo.Object),
            new GameCountByYearExecutor(repo.Object),
            new GameCountByResultExecutor(repo.Object),
            new GameCountByPlayerExecutor(repo.Object),
            new AverageMaterialByPlayerAtMoveExecutor(repo.Object)
        });

        Assert.Contains("AverageMaterialByYearAndColour", sut.MetricKeys);
        Assert.Contains("KnightMoveDestinationFrequency", sut.MetricKeys);
        Assert.Contains("GameCountByEco", sut.MetricKeys);
        Assert.Contains("GameCountByYear", sut.MetricKeys);
        Assert.Contains("GameCountByResult", sut.MetricKeys);
        Assert.Contains("GameCountByPlayer", sut.MetricKeys);
        Assert.Contains("AverageMaterialByPlayerAtMove", sut.MetricKeys);
        Assert.Equal(7, sut.MetricKeys.Count);
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
            BlackPlayerSurname = "Karpov",
            BlackPlayerForenames = "Anatoly",
            Eco = "B90"
        };
        var sut = new GameCountByYearExecutor(repo.Object);

        await sut.ExecuteAsync(query);

        repo.Verify(r => r.GetGameCountsByYearAsync(
            It.Is<AnalyticsQuery>(q =>
                q.MinGameYear == 1980
                && q.MaxGameYear == 1990
                && q.BlackPlayerSurname == "Karpov"
                && q.BlackPlayerForenames == "Anatoly"
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
            WhitePlayerSurname = "Kasparov",
            WhitePlayerForenames = "Garry",
            Eco = "B90"
        };
        var sut = new GameCountByResultExecutor(repo.Object);

        await sut.ExecuteAsync(query);

        repo.Verify(r => r.GetGameCountsByResultAsync(
            It.Is<AnalyticsQuery>(q =>
                q.MinGameYear == 1980
                && q.MaxGameYear == 1990
                && q.WhitePlayerSurname == "Kasparov"
                && q.WhitePlayerForenames == "Garry"
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
            WhitePlayerSurname = "Kasparov",
            WhitePlayerForenames = "Garry",
            Eco = "B90"
        };
        var sut = new GameCountByPlayerExecutor(repo.Object);

        await sut.ExecuteAsync(query);

        repo.Verify(r => r.GetGameCountsByPlayerAsync(
            It.Is<AnalyticsQuery>(q =>
                q.MinGameYear == 1980
                && q.MaxGameYear == 1990
                && q.WhitePlayerSurname == "Kasparov"
                && q.WhitePlayerForenames == "Garry"
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
}
