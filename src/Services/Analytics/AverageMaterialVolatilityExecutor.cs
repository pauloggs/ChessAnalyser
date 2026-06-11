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
        var rows = (await _repository.GetAverageMaterialVolatilityAsync(query, cancellationToken).ConfigureAwait(false)).ToList();

        if (CorpusBenchmarkQueryDefaults.IncludeCorpusBenchmark(query) && rows.Count > 0)
        {
            var perPlayer = await _repository
                .GetPerPlayerAverageMaterialVolatilityAsync(query, cancellationToken)
                .ConfigureAwait(false);

            var minGames = CorpusBenchmarkQueryDefaults.BenchmarkMinGames(query);
            rows = rows
                .Select(r => EnrichWithBenchmark(r, query, perPlayer, minGames))
                .ToList();
        }

        var includeBenchmark = CorpusBenchmarkQueryDefaults.IncludeCorpusBenchmark(query);
        return new AnalyticsTableResult
        {
            ColumnNames = includeBenchmark
                ? ["Player", "GameCount", "AverageMaterialVolatility", "CorpusAverage", "DeltaFromCorpus", "CorpusPercentile", "CorpusEligiblePlayerCount"]
                : ["Player", "GameCount", "AverageMaterialVolatility"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)FormatRow(r, includeBenchmark).ToList()).ToList()
        };
    }

    private AverageMaterialVolatilityRow EnrichWithBenchmark(
        AverageMaterialVolatilityRow row,
        AnalyticsQuery query,
        IReadOnlyList<PlayerStylePerPlayerMetricRow> perPlayer,
        int benchmarkMinGames)
    {
        var benchmark = _corpusBenchmarkCalculator.Compute(
            row.AverageMaterialVolatility,
            query.PlayerSurname!,
            query.PlayerForenames,
            perPlayer,
            benchmarkMinGames);

        return new AverageMaterialVolatilityRow
        {
            PlayerSurname = row.PlayerSurname,
            PlayerForenames = row.PlayerForenames,
            GameCount = row.GameCount,
            AverageMaterialVolatility = row.AverageMaterialVolatility,
            CorpusAverage = benchmark.CorpusAverage,
            DeltaFromCorpus = benchmark.DeltaFromCorpus,
            CorpusPercentile = benchmark.CorpusPercentile,
            CorpusEligiblePlayerCount = benchmark.CorpusEligiblePlayerCount
        };
    }

    private static IReadOnlyList<object?> FormatRow(AverageMaterialVolatilityRow r, bool includeBenchmark)
    {
        if (!includeBenchmark)
        {
            return
            [
                FormatPlayer(r),
                r.GameCount,
                r.AverageMaterialVolatility
            ];
        }

        return
        [
            FormatPlayer(r),
            r.GameCount,
            r.AverageMaterialVolatility,
            r.CorpusAverage,
            r.DeltaFromCorpus,
            r.CorpusPercentile,
            r.CorpusEligiblePlayerCount
        ];
    }

    private static string FormatPlayer(AverageMaterialVolatilityRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
