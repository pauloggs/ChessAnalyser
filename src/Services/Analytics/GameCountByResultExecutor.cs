using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Counts games by normalized result with the shared game filters.
/// </summary>
public sealed class GameCountByResultExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "GameCountByResult";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var rows = await _repository.GetGameCountsByResultAsync(query, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(GameCountByResultRow r) =>
            new object?[] { r.Result, r.GameCount };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Result", "GameCount"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }
}
