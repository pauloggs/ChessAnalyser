using Interfaces.Analytics;

namespace Services.Analytics;

public sealed class CorpusBenchmarkCalculator : ICorpusBenchmarkCalculator
{
    public CorpusBenchmarkResult Compute(
        double? subjectValue,
        string subjectSurname,
        string? subjectForenames,
        IReadOnlyList<PlayerStylePerPlayerMetricRow> perPlayerRows,
        int benchmarkMinGames)
    {
        ArgumentNullException.ThrowIfNull(subjectSurname);
        ArgumentNullException.ThrowIfNull(perPlayerRows);

        if (benchmarkMinGames < 1)
            throw new ArgumentOutOfRangeException(nameof(benchmarkMinGames));

        var eligible = perPlayerRows
            .Where(r => r.GameCount >= benchmarkMinGames && r.MetricValue is not null)
            .ToList();

        var corpus = eligible
            .Where(r => !PlayerIdentityMatches(r, subjectSurname, subjectForenames))
            .Select(r => r.MetricValue!.Value)
            .ToList();

        if (corpus.Count == 0 || subjectValue is null)
        {
            return new CorpusBenchmarkResult
            {
                CorpusEligiblePlayerCount = corpus.Count
            };
        }

        var average = corpus.Average();
        var percentile = ComputePercentile(subjectValue.Value, corpus);

        return new CorpusBenchmarkResult
        {
            CorpusAverage = average,
            DeltaFromCorpus = subjectValue.Value - average,
            CorpusPercentile = percentile,
            CorpusEligiblePlayerCount = corpus.Count
        };
    }

    public static double ComputePercentile(double subjectValue, IReadOnlyList<double> corpusValues)
    {
        if (corpusValues.Count == 0)
            throw new ArgumentException("Corpus must not be empty.", nameof(corpusValues));

        if (subjectValue < corpusValues.Min())
            return 0;

        if (subjectValue > corpusValues.Max())
            return 100;

        var less = corpusValues.Count(v => v < subjectValue);
        return 100.0 * less / corpusValues.Count;
    }

    private static bool PlayerIdentityMatches(
        PlayerStylePerPlayerMetricRow row, string surname, string? forenames)
    {
        if (!string.Equals(row.PlayerSurname, surname, StringComparison.Ordinal))
            return false;

        if (forenames is null)
            return true;

        return string.Equals(row.PlayerForenames ?? string.Empty, forenames, StringComparison.Ordinal);
    }
}
