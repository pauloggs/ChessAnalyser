using Interfaces.Analytics;

namespace Analyser.Models;

/// <summary>
/// JSON-friendly tabular metric result: <see cref="ColumnNames"/> plus one array per row (cell order matches columns).
/// </summary>
public sealed class AnalyticsTableResponse
{
    public required IReadOnlyList<string> ColumnNames { get; init; }

    /// <summary>Each inner array aligns with <see cref="ColumnNames"/> by index.</summary>
    public required IReadOnlyList<object?[]> Rows { get; init; }

    public static AnalyticsTableResponse FromResult(AnalyticsTableResult result) =>
        new()
        {
            ColumnNames = result.ColumnNames.ToList(),
            Rows = result.Rows.Select(r => r.ToArray()).ToList()
        };
}
