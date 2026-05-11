using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Summarizes results from each player's perspective with the shared game filters.
/// </summary>
public sealed class PlayerResultSummaryExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "PlayerResultSummary";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var rows = await _repository.GetPlayerResultSummariesAsync(query, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(PlayerResultSummaryRow r) =>
            new object?[]
            {
                FormatPlayer(r),
                r.WinCount,
                r.LossCount,
                r.DrawCount,
                r.UnknownCount,
                r.TotalGameCount,
                r.Score
            };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Player", "WinCount", "LossCount", "DrawCount", "UnknownCount", "TotalGameCount", "Score"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }

    private static string FormatPlayer(PlayerResultSummaryRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
