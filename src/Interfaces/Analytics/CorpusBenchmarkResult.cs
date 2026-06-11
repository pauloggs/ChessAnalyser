namespace Interfaces.Analytics;

/// <summary>
/// Corpus-relative benchmark columns (DESIGN §12, PLAN §12.7).
/// </summary>
public sealed class CorpusBenchmarkResult
{
    public double? CorpusAverage { get; init; }

    public double? DeltaFromCorpus { get; init; }

    public double? CorpusPercentile { get; init; }

    public int? CorpusEligiblePlayerCount { get; init; }
}
