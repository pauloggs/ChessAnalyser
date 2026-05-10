namespace Interfaces.Analytics;

/// <summary>
/// Tabular result for a metric execution (column names + row values, PLAN §5.1).
/// </summary>
public sealed class AnalyticsTableResult
{
    public required IReadOnlyList<string> ColumnNames { get; init; }

    public required IReadOnlyList<IReadOnlyList<object?>> Rows { get; init; }

    public static AnalyticsTableResult Empty(params string[] columnNames) =>
        new()
        {
            ColumnNames = columnNames,
            Rows = Array.Empty<IReadOnlyList<object?>>()
        };
}
