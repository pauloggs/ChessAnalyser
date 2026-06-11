using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Mean half-move of the filtered player's first castle (PLAN §12.6 Phase 1).
/// </summary>
public sealed class AverageCastlingPlyExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "AverageCastlingPly";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var rows = await _repository.GetAverageCastlingPlyAsync(query, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(AverageCastlingPlyRow r) =>
            new object?[]
            {
                FormatPlayer(r),
                r.GamesWithCastling,
                r.AverageCastlingPly
            };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Player", "GamesWithCastling", "AverageCastlingPly"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }

    private static string FormatPlayer(AverageCastlingPlyRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
