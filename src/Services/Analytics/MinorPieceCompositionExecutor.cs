using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Mean bishops-minus-knights count for a player across a ply window (PLAN §12.6 Phase 2).
/// </summary>
public sealed class MinorPieceCompositionExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "MinorPieceComposition";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var (minPly, maxPly) = PlayerStylePlyWindow.Resolve(query);
        var rows = await _repository.GetMinorPieceCompositionAsync(query, minPly, maxPly, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(MinorPieceCompositionRow r) =>
            new object?[]
            {
                FormatPlayer(r),
                r.GameCount,
                r.AverageMinorPieceDelta,
                r.MinPlyIndex,
                r.MaxPlyIndex
            };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Player", "GameCount", "AverageMinorPieceDelta", "MinPlyIndex", "MaxPlyIndex"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }

    private static string FormatPlayer(MinorPieceCompositionRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
