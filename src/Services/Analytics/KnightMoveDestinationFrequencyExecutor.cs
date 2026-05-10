using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Reference metric: frequency of knight half-moves by destination square (PLAN §5.3.4).
/// </summary>
public sealed class KnightMoveDestinationFrequencyExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "KnightMoveDestinationFrequency";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var rows = await _repository.GetKnightDestinationCountsAsync(query, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(KnightDestinationCountRow r) =>
            new object?[] { r.ToSquare, r.MoveCount };

        return new AnalyticsTableResult
        {
            ColumnNames = ["ToSquare", "MoveCount"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }
}
