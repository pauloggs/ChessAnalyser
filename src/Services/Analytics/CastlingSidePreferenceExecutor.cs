using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Kingside vs queenside preference on the filtered player's first castle (PLAN §12.6 Phase 5).
/// </summary>
public sealed class CastlingSidePreferenceExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "CastlingSidePreference";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var rows = await _repository.GetCastlingSidePreferenceAsync(query, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(CastlingSidePreferenceRow r) =>
            new object?[]
            {
                FormatPlayer(r),
                r.GamesWithCastling,
                r.KingsideRate,
                r.QueensideRate
            };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Player", "GamesWithCastling", "KingsideRate", "QueensideRate"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }

    private static string FormatPlayer(CastlingSidePreferenceRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
