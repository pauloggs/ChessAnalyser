using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Counts games by parsed calendar year with the shared game filters.
/// </summary>
public sealed class GameCountByYearExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "GameCountByYear";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var rows = await _repository.GetGameCountsByYearAsync(query, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(GameCountByYearRow r) =>
            new object?[] { r.GameYear, r.GameCount };

        return new AnalyticsTableResult
        {
            ColumnNames = ["GameYear", "GameCount"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }
}
