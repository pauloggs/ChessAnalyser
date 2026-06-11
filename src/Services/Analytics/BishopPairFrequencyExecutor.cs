using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Mean share of plies in a window where the player retains both bishops (PLAN §12.6 Phase 2).
/// </summary>
public sealed class BishopPairFrequencyExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "BishopPairFrequency";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var (minPly, maxPly) = PlayerStylePlyWindow.Resolve(query);
        var rows = await _repository.GetBishopPairFrequencyAsync(query, minPly, maxPly, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(BishopPairFrequencyRow r) =>
            new object?[]
            {
                FormatPlayer(r),
                r.GameCount,
                r.AverageBishopPairFrequency,
                r.MinPlyIndex,
                r.MaxPlyIndex
            };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Player", "GameCount", "AverageBishopPairFrequency", "MinPlyIndex", "MaxPlyIndex"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }

    private static string FormatPlayer(BishopPairFrequencyRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
