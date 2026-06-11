using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Mean ply of the filtered player's first queen move (PLAN §12.6 Phase 6).
/// </summary>
public sealed class FirstQueenMovePlyExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "FirstQueenMovePly";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var rows = await _repository.GetFirstQueenMovePlyAsync(query, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(FirstQueenMovePlyRow r) =>
            new object?[]
            {
                FormatPlayer(r),
                r.GamesWithQueenMove,
                r.AverageFirstQueenMovePly
            };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Player", "GamesWithQueenMove", "AverageFirstQueenMovePly"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }

    private static string FormatPlayer(FirstQueenMovePlyRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
