using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Counts player appearances with the shared game filters.
/// </summary>
public sealed class GameCountByPlayerExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "GameCountByPlayer";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var rows = await _repository.GetGameCountsByPlayerAsync(query, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(GameCountByPlayerRow r) =>
            new object?[] { FormatPlayer(r), r.WhiteGameCount, r.BlackGameCount, r.TotalGameCount };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Player", "WhiteGameCount", "BlackGameCount", "TotalGameCount"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }

    private static string FormatPlayer(GameCountByPlayerRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
