using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Mean per-game share of moves landing in the opponent's half (PLAN §12.6 Phase 4).
/// </summary>
public sealed class ForwardMoveRateExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "ForwardMoveRate";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var rows = await _repository.GetForwardMoveRateAsync(query, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(ForwardMoveRateRow r) =>
            new object?[]
            {
                FormatPlayer(r),
                r.GameCount,
                r.AverageForwardMoveRate
            };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Player", "GameCount", "AverageForwardMoveRate"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }

    private static string FormatPlayer(ForwardMoveRateRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
