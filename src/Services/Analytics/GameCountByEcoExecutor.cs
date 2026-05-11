using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Counts games by ECO code with the shared game filters.
/// </summary>
public sealed class GameCountByEcoExecutor(IChessRepository repository) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "GameCountByEco";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var rows = await _repository.GetGameCountsByEcoAsync(query, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<object?> Row(GameCountByEcoRow r) =>
            new object?[] { r.Eco, r.GameCount };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Eco", "GameCount"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }
}
