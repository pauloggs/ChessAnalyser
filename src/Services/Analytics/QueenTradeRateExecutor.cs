using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Share of games with queens off the board on or before a ply threshold (PLAN §12.6 Phase 3).
/// </summary>
public sealed class QueenTradeRateExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "QueenTradeRate";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var queenTradeMaxPly = QueenTradeMaxPlyDefaults.Resolve(query);
        var rows = await _repository.GetQueenTradeRateAsync(query, queenTradeMaxPly, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(QueenTradeRateRow r) =>
            new object?[]
            {
                FormatPlayer(r),
                r.GameCount,
                r.QueenTradeRate,
                r.QueenTradeMaxPly
            };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Player", "GameCount", "QueenTradeRate", "QueenTradeMaxPly"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }

    private static string FormatPlayer(QueenTradeRateRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
