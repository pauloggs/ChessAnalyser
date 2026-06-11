using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Share of the filtered player's draws that end at or below a ply threshold (PLAN §12.6 Phase 7).
/// </summary>
public sealed class ShortDrawRateExecutor(
    IChessRepository repository,
    ICorpusBenchmarkCalculator corpusBenchmarkCalculator) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly ICorpusBenchmarkCalculator _corpusBenchmarkCalculator =
        corpusBenchmarkCalculator ?? throw new ArgumentNullException(nameof(corpusBenchmarkCalculator));

    public string MetricKey => "ShortDrawRate";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var shortDrawMaxPly = ShortDrawMaxPlyDefaults.Resolve(query);
        var rows = (await _repository.GetShortDrawRateAsync(query, shortDrawMaxPly, cancellationToken).ConfigureAwait(false)).ToList();

        if (CorpusBenchmarkQueryDefaults.IncludeCorpusBenchmark(query) && rows.Count > 0)
        {
            var perPlayer = await _repository
                .GetPerPlayerShortDrawRateAsync(query, shortDrawMaxPly, cancellationToken)
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
                ?
                [
                    "Player", "DrawCount", "ShortDrawRate", "ShortDrawMaxPly",
                    "CorpusAverage", "DeltaFromCorpus", "CorpusPercentile", "CorpusEligiblePlayerCount"
                ]
                : ["Player", "DrawCount", "ShortDrawRate", "ShortDrawMaxPly"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)FormatRow(r, includeBenchmark).ToList()).ToList()
        };
    }

    private ShortDrawRateRow EnrichWithBenchmark(
        ShortDrawRateRow row,
        AnalyticsQuery query,
        IReadOnlyList<PlayerStylePerPlayerMetricRow> perPlayer,
        int benchmarkMinGames)
    {
        var benchmark = _corpusBenchmarkCalculator.Compute(
            row.ShortDrawRate,
            query.PlayerSurname!,
            query.PlayerForenames,
            perPlayer,
            benchmarkMinGames);

        return new ShortDrawRateRow
        {
            PlayerSurname = row.PlayerSurname,
            PlayerForenames = row.PlayerForenames,
            DrawCount = row.DrawCount,
            ShortDrawRate = row.ShortDrawRate,
            ShortDrawMaxPly = row.ShortDrawMaxPly,
            CorpusAverage = benchmark.CorpusAverage,
            DeltaFromCorpus = benchmark.DeltaFromCorpus,
            CorpusPercentile = benchmark.CorpusPercentile,
            CorpusEligiblePlayerCount = benchmark.CorpusEligiblePlayerCount
        };
    }

    private static IReadOnlyList<object?> FormatRow(ShortDrawRateRow r, bool includeBenchmark)
    {
        if (!includeBenchmark)
        {
            return
            [
                FormatPlayer(r),
                r.DrawCount,
                r.ShortDrawRate,
                r.ShortDrawMaxPly
            ];
        }

        return
        [
            FormatPlayer(r),
            r.DrawCount,
            r.ShortDrawRate,
            r.ShortDrawMaxPly,
            r.CorpusAverage,
            r.DeltaFromCorpus,
            r.CorpusPercentile,
            r.CorpusEligiblePlayerCount
        ];
    }

    private static string FormatPlayer(ShortDrawRateRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
