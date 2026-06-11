using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Count of distinct ECO codes in games involving the filtered player (PLAN §12.6 Phase 8).
/// </summary>
public sealed class EcoDiversityExecutor(
    IChessRepository repository,
    ICorpusBenchmarkCalculator corpusBenchmarkCalculator) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly ICorpusBenchmarkCalculator _corpusBenchmarkCalculator =
        corpusBenchmarkCalculator ?? throw new ArgumentNullException(nameof(corpusBenchmarkCalculator));

    public string MetricKey => "EcoDiversity";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var rows = (await _repository.GetEcoDiversityAsync(query, cancellationToken).ConfigureAwait(false)).ToList();

        if (CorpusBenchmarkQueryDefaults.IncludeCorpusBenchmark(query) && rows.Count > 0)
        {
            var perPlayer = await _repository
                .GetPerPlayerEcoDiversityAsync(query, cancellationToken)
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
                    "Player", "GameCount", "EcoDiversity",
                    "CorpusAverage", "DeltaFromCorpus", "CorpusPercentile", "CorpusEligiblePlayerCount"
                ]
                : ["Player", "GameCount", "EcoDiversity"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)FormatRow(r, includeBenchmark).ToList()).ToList()
        };
    }

    private EcoDiversityRow EnrichWithBenchmark(
        EcoDiversityRow row,
        AnalyticsQuery query,
        IReadOnlyList<PlayerStylePerPlayerMetricRow> perPlayer,
        int benchmarkMinGames)
    {
        var benchmark = _corpusBenchmarkCalculator.Compute(
            row.EcoDiversity,
            query.PlayerSurname!,
            query.PlayerForenames,
            perPlayer,
            benchmarkMinGames);

        return new EcoDiversityRow
        {
            PlayerSurname = row.PlayerSurname,
            PlayerForenames = row.PlayerForenames,
            GameCount = row.GameCount,
            EcoDiversity = row.EcoDiversity,
            CorpusAverage = benchmark.CorpusAverage,
            DeltaFromCorpus = benchmark.DeltaFromCorpus,
            CorpusPercentile = benchmark.CorpusPercentile,
            CorpusEligiblePlayerCount = benchmark.CorpusEligiblePlayerCount
        };
    }

    private static IReadOnlyList<object?> FormatRow(EcoDiversityRow r, bool includeBenchmark)
    {
        if (!includeBenchmark)
        {
            return
            [
                FormatPlayer(r),
                r.GameCount,
                r.EcoDiversity
            ];
        }

        return
        [
            FormatPlayer(r),
            r.GameCount,
            r.EcoDiversity,
            r.CorpusAverage,
            r.DeltaFromCorpus,
            r.CorpusPercentile,
            r.CorpusEligiblePlayerCount
        ];
    }

    private static string FormatPlayer(EcoDiversityRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
