using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Mean per-game share of moves landing on central squares d4, d5, e4, e5 (PLAN §12.6 Phase 4).
/// </summary>
public sealed class CentreMoveRateExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "CentreMoveRate";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var rows = await _repository.GetCentreMoveRateAsync(query, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(CentreMoveRateRow r) =>
            new object?[]
            {
                FormatPlayer(r),
                r.GameCount,
                r.AverageCentreMoveRate
            };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Player", "GameCount", "AverageCentreMoveRate"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }

    private static string FormatPlayer(CentreMoveRateRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
