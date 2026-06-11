namespace Interfaces.Analytics;

/// <summary>
/// Computes corpus-relative benchmark fields from per-player metric aggregates (DESIGN §12).
/// </summary>
public interface ICorpusBenchmarkCalculator
{
    CorpusBenchmarkResult Compute(
        double? subjectValue,
        string subjectSurname,
        string? subjectForenames,
        IReadOnlyList<PlayerStylePerPlayerMetricRow> perPlayerRows,
        int benchmarkMinGames);
}
