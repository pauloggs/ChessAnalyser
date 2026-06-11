using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Proportion of filtered games where the player never castled (PLAN §12.6 Phase 5).
/// </summary>
public sealed class UncastledKingRateExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "UncastledKingRate";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var rows = await _repository.GetUncastledKingRateAsync(query, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(UncastledKingRateRow r) =>
            new object?[]
            {
                FormatPlayer(r),
                r.GameCount,
                r.UncastledKingRate
            };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Player", "GameCount", "UncastledKingRate"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }

    private static string FormatPlayer(UncastledKingRateRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
