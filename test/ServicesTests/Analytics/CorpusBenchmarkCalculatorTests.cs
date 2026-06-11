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
            benchmarkMinGames: 30);

        Assert.Null(result.CorpusAverage);
        Assert.Null(result.DeltaFromCorpus);
        Assert.Null(result.CorpusPercentile);
        Assert.Equal(0, result.CorpusEligiblePlayerCount);
    }

    [Fact]
    public void Compute_ExcludesSubjectFromCorpusAverage()
    {
        var result = _sut.Compute(
            1.4,
            "Fischer",
            "Robert James",
            new List<PlayerStylePerPlayerMetricRow>
            {
                new() { PlayerSurname = "Fischer", PlayerForenames = "Robert James", GameCount = 50, MetricValue = 1.4 },
                new() { PlayerSurname = "Petrosian", PlayerForenames = "Tigran", GameCount = 40, MetricValue = 1.0 },
                new() { PlayerSurname = "Tal", PlayerForenames = "Mikhail", GameCount = 35, MetricValue = 1.8 }
            },
            benchmarkMinGames: 30);

        Assert.Equal(1.4, result.CorpusAverage);
        Assert.Equal(0, result.DeltaFromCorpus);
        Assert.Equal(50, result.CorpusPercentile);
        Assert.Equal(2, result.CorpusEligiblePlayerCount);
    }

    [Fact]
    public void Compute_IgnoresPlayersBelowBenchmarkMinGames()
    {
        var result = _sut.Compute(
            2.0,
            "Fischer",
            null,
            new List<PlayerStylePerPlayerMetricRow>
            {
                new() { PlayerSurname = "Fischer", GameCount = 50, MetricValue = 2.0 },
                new() { PlayerSurname = "Short", GameCount = 5, MetricValue = 0.5 }
            },
            benchmarkMinGames: 30);

        Assert.Null(result.CorpusAverage);
        Assert.Equal(0, result.CorpusEligiblePlayerCount);
    }

    [Theory]
    [InlineData(1.0, new[] { 1.0, 2.0, 3.0 }, 0)]
    [InlineData(3.0, new[] { 1.0, 2.0, 3.0 }, 66.666666666666686)]
    [InlineData(2.0, new[] { 1.0, 2.0, 3.0 }, 33.333333333333336)]
    [InlineData(4.0, new[] { 1.0, 2.0, 3.0 }, 100)]
    public void ComputePercentile_RanksSubjectAgainstCorpus(double subject, double[] corpus, double expected)
    {
        var percentile = CorpusBenchmarkCalculator.ComputePercentile(subject, corpus);
        Assert.Equal(expected, percentile, precision: 10);
    }
}
