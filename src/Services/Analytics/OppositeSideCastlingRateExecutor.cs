using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Share of filtered games where both sides castled to opposite wings (PLAN §12.6 Phase 5).
/// </summary>
public sealed class OppositeSideCastlingRateExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "OppositeSideCastlingRate";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var rows = await _repository.GetOppositeSideCastlingRateAsync(query, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(OppositeSideCastlingRateRow r) =>
            new object?[]
            {
                FormatPlayer(r),
                r.EligibleGameCount,
                r.OppositeSideCastlingRate
            };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Player", "EligibleGameCount", "OppositeSideCastlingRate"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }

    private static string FormatPlayer(OppositeSideCastlingRateRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
