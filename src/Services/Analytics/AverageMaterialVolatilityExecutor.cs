using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Mean per-game standard deviation of signed material balance (PLAN §12.6 Phase 1).
/// </summary>
public sealed class AverageMaterialVolatilityExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "AverageMaterialVolatility";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var rows = await _repository.GetAverageMaterialVolatilityAsync(query, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(AverageMaterialVolatilityRow r) =>
            new object?[]
            {
                FormatPlayer(r),
                r.GameCount,
                r.AverageMaterialVolatility
            };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Player", "GameCount", "AverageMaterialVolatility"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }

    private static string FormatPlayer(AverageMaterialVolatilityRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
