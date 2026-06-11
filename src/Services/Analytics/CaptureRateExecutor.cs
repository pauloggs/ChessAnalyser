using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Mean per-game capture rate for the filtered player's moves (PLAN §12.6 Phase 3).
/// </summary>
public sealed class CaptureRateExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "CaptureRate";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var rows = await _repository.GetCaptureRateAsync(query, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(CaptureRateRow r) =>
            new object?[]
            {
                FormatPlayer(r),
                r.GameCount,
                r.AverageCaptureRate
            };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Player", "GameCount", "AverageCaptureRate"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }

    private static string FormatPlayer(CaptureRateRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
