using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Reference metric: average <see cref="Interfaces.DTO.GamePositionSummary.WhiteMaterial"/> /
/// <see cref="Interfaces.DTO.GamePositionSummary.BlackMaterial"/> at a fixed ply, grouped by <c>GameYear</c> (PLAN §5.3.4).
/// </summary>
public sealed class AverageMaterialByYearAndColourExecutor(IChessRepository repository) : IMetricExecutor
{
    private const int DefaultSummaryPlyIndex = 4;

    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "AverageMaterialByYearAndColour";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var plyIndex = query.SummaryPlyIndex ?? DefaultSummaryPlyIndex;
        var rows = await _repository.GetMaterialAveragesByYearAtPlyAsync(query, plyIndex, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(MaterialAverageByYearRow r) =>
            new object?[] { r.GameYear, r.AvgWhiteMaterial, r.AvgBlackMaterial, r.GameCount };

        return new AnalyticsTableResult
        {
            ColumnNames = ["GameYear", "AvgWhiteMaterial", "AvgBlackMaterial", "GameCount"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }
}
