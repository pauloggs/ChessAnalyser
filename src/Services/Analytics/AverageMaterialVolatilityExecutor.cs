using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Mean per-game standard deviation of signed material balance (PLAN §12.6 Phase 1).
/// </summary>
public sealed class AverageMaterialVolatilityExecutor(
    IChessRepository repository,
    ICorpusBenchmarkCalculator corpusBenchmarkCalculator) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly ICorpusBenchmarkCalculator _corpusBenchmarkCalculator =
        corpusBenchmarkCalculator ?? throw new ArgumentNullException(nameof(corpusBenchmarkCalculator));

    public string MetricKey => "AverageMaterialVolatility";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var rows = await _repository.GetAverageMaterialVolatilityAsync(query, cancellationToken).ConfigureAwait(false);
        var includeBenchmark = CorpusBenchmarkQueryDefaults.IncludeBenchmark(query);
        IReadOnlyList<PlayerStylePerPlayerMetricRow>? corpusRows = null;

        if (includeBenchmark)
        {
            corpusRows = await _repository
                .GetPerPlayerAverageMaterialVolatilityAsync(query, cancellationToken)
                .ConfigureAwait(false);
        }

        var benchmarkMinGames = CorpusBenchmarkQueryDefaults.BenchmarkMinGames(query);
        var columnNames = new List<string> { "Player", "GameCount", "AverageMaterialVolatility" };
        if (includeBenchmark)
        {
            columnNames.AddRange(
            [
                "CorpusAverage",
                "DeltaFromCorpus",
                "CorpusPercentile",
                "CorpusEligiblePlayerCount"
            ]);
        }

        IReadOnlyList<object?> Row(AverageMaterialVolatilityRow r)
        {
            var values = new List<object?>
            {
                FormatPlayer(r),
                r.GameCount,
                r.AverageMaterialVolatility
            };

            if (!includeBenchmark)
                return values;

            var benchmark = _corpusBenchmarkCalculator.Compute(
                r.AverageMaterialVolatility,
                query.PlayerSurname!,
                query.PlayerForenames,
                corpusRows ?? [],
                benchmarkMinGames);

            values.Add(benchmark.CorpusAverage);
            values.Add(benchmark.DeltaFromCorpus);
            values.Add(benchmark.CorpusPercentile);
            values.Add(benchmark.CorpusEligiblePlayerCount);
            return values;
        }

        return new AnalyticsTableResult
        {
            ColumnNames = columnNames,
            Rows = rows.Select(r => Row(r).ToList()).ToList()
        };
    }

    private static string FormatPlayer(AverageMaterialVolatilityRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
