using Interfaces.Analytics;
using Services.Analytics;

namespace ServicesTests.Analytics;

public class CorpusBenchmarkCalculatorTests
{
    private readonly CorpusBenchmarkCalculator _sut = new();

    [Fact]
    public void Compute_ReturnsNullBenchmark_WhenCorpusEmpty()
    {
        var result = _sut.Compute(
            1.5,
            "Fischer",
            "Robert James",
            new List<PlayerStylePerPlayerMetricRow>
            {
                new() { PlayerSurname = "Fischer", PlayerForenames = "Robert James", GameCount = 50, MetricValue = 1.5 }
            },
            30);

        Assert.Null(result.CorpusAverage);
        Assert.Null(result.DeltaFromCorpus);
        Assert.Null(result.CorpusPercentile);
        Assert.Equal(0, result.CorpusEligiblePlayerCount);
    }

    [Fact]
    public void Compute_ExcludesSubjectFromCorpusMeanAndPercentile()
    {
        var result = _sut.Compute(
            3.0,
            "Fischer",
            "Robert James",
            new List<PlayerStylePerPlayerMetricRow>
            {
                new() { PlayerSurname = "Fischer", PlayerForenames = "Robert James", GameCount = 50, MetricValue = 3.0 },
                new() { PlayerSurname = "Tal", PlayerForenames = "Mikhail", GameCount = 40, MetricValue = 1.0 },
                new() { PlayerSurname = "Petrosian", PlayerForenames = "Tigran", GameCount = 40, MetricValue = 2.0 }
            },
            30);

        Assert.Equal(1.5, result.CorpusAverage);
        Assert.Equal(1.5, result.DeltaFromCorpus);
        Assert.Equal(100, result.CorpusPercentile);
        Assert.Equal(2, result.CorpusEligiblePlayerCount);
    }

    [Fact]
    public void Compute_RespectsBenchmarkMinGames()
    {
        var result = _sut.Compute(
            2.0,
            "Fischer",
            null,
            new List<PlayerStylePerPlayerMetricRow>
            {
                new() { PlayerSurname = "Fischer", GameCount = 50, MetricValue = 2.0 },
                new() { PlayerSurname = "Tal", GameCount = 10, MetricValue = 5.0 },
                new() { PlayerSurname = "Petrosian", GameCount = 40, MetricValue = 1.0 }
            },
            30);

        Assert.Equal(1.0, result.CorpusAverage);
        Assert.Equal(1, result.CorpusEligiblePlayerCount);
    }

    [Theory]
    [InlineData(0.5, new[] { 1.0, 2.0, 3.0 }, 0)]
    [InlineData(2.0, new[] { 1.0, 2.0, 3.0 }, 33.333333333333336)]
    [InlineData(4.0, new[] { 1.0, 2.0, 3.0 }, 100)]
    public void ComputePercentile_UsesStrictLessThanOrdering(double subject, double[] corpus, double expected)
    {
        var percentile = CorpusBenchmarkCalculator.ComputePercentile(subject, corpus);
        Assert.Equal(expected, percentile, precision: 10);
    }
}
