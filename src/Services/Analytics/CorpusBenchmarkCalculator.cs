using Interfaces.Analytics;

namespace Services.Analytics;

/// <summary>
/// Leave-one-out corpus mean and percentile for style metrics (DESIGN §12).
/// </summary>
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

        if (subjectValue is null)
            return Empty();

        var eligible = perPlayerRows
            .Where(r => r.GameCount >= benchmarkMinGames && r.MetricValue.HasValue)
            .ToList();

        var corpusValues = eligible
            .Where(r => !IsSubject(r, subjectSurname, subjectForenames))
            .Select(r => r.MetricValue!.Value)
            .ToList();

        var eligiblePlayerCount = eligible.Count(r => !IsSubject(r, subjectSurname, subjectForenames));

        if (corpusValues.Count == 0)
        {
            return new CorpusBenchmarkResult
            {
                CorpusEligiblePlayerCount = eligiblePlayerCount
            };
        }

        var corpusAverage = corpusValues.Average();

        return new CorpusBenchmarkResult
        {
            CorpusAverage = corpusAverage,
            DeltaFromCorpus = subjectValue.Value - corpusAverage,
            CorpusPercentile = ComputePercentile(subjectValue.Value, corpusValues),
            CorpusEligiblePlayerCount = eligiblePlayerCount
        };
    }

    public static double ComputePercentile(double subjectValue, IReadOnlyList<double> corpusValues)
    {
        ArgumentNullException.ThrowIfNull(corpusValues);
        if (corpusValues.Count == 0)
            return double.NaN;

        if (subjectValue < corpusValues.Min())
            return 0;

        if (subjectValue > corpusValues.Max())
            return 100;

        var less = corpusValues.Count(v => v < subjectValue);
        return 100.0 * less / corpusValues.Count;
    }

    private static bool IsSubject(PlayerStylePerPlayerMetricRow row, string subjectSurname, string? subjectForenames)
    {
        if (!string.Equals(row.PlayerSurname, subjectSurname, StringComparison.Ordinal))
            return false;

        if (subjectForenames is null)
            return true;

        return string.Equals(row.PlayerForenames ?? string.Empty, subjectForenames, StringComparison.Ordinal);
    }

    private static CorpusBenchmarkResult Empty() => new();
}
