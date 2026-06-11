using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Mean share of plies in a window where the player retains both bishops (PLAN §12.6 Phase 2).
/// </summary>
public sealed class BishopPairFrequencyExecutor(
    IChessRepository repository,
    ICorpusBenchmarkCalculator corpusBenchmarkCalculator) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly ICorpusBenchmarkCalculator _corpusBenchmarkCalculator =
        corpusBenchmarkCalculator ?? throw new ArgumentNullException(nameof(corpusBenchmarkCalculator));

    public string MetricKey => "BishopPairFrequency";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var (minPly, maxPly) = PlayerStylePlyWindow.Resolve(query);
        var rows = (await _repository.GetBishopPairFrequencyAsync(query, minPly, maxPly, cancellationToken).ConfigureAwait(false)).ToList();

        if (CorpusBenchmarkQueryDefaults.IncludeCorpusBenchmark(query) && rows.Count > 0)
        {
            var perPlayer = await _repository
                .GetPerPlayerBishopPairFrequencyAsync(query, minPly, maxPly, cancellationToken)
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
                    "Player", "GameCount", "AverageBishopPairFrequency", "MinPlyIndex", "MaxPlyIndex",
                    "CorpusAverage", "DeltaFromCorpus", "CorpusPercentile", "CorpusEligiblePlayerCount"
                ]
                : ["Player", "GameCount", "AverageBishopPairFrequency", "MinPlyIndex", "MaxPlyIndex"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)FormatRow(r, includeBenchmark).ToList()).ToList()
        };
    }

    private BishopPairFrequencyRow EnrichWithBenchmark(
        BishopPairFrequencyRow row,
        AnalyticsQuery query,
        IReadOnlyList<PlayerStylePerPlayerMetricRow> perPlayer,
        int benchmarkMinGames)
    {
        var benchmark = _corpusBenchmarkCalculator.Compute(
            row.AverageBishopPairFrequency,
            query.PlayerSurname!,
            query.PlayerForenames,
            perPlayer,
            benchmarkMinGames);

        return new BishopPairFrequencyRow
        {
            PlayerSurname = row.PlayerSurname,
            PlayerForenames = row.PlayerForenames,
            GameCount = row.GameCount,
            AverageBishopPairFrequency = row.AverageBishopPairFrequency,
            MinPlyIndex = row.MinPlyIndex,
            MaxPlyIndex = row.MaxPlyIndex,
            CorpusAverage = benchmark.CorpusAverage,
            DeltaFromCorpus = benchmark.DeltaFromCorpus,
            CorpusPercentile = benchmark.CorpusPercentile,
            CorpusEligiblePlayerCount = benchmark.CorpusEligiblePlayerCount
        };
    }

    private static IReadOnlyList<object?> FormatRow(BishopPairFrequencyRow r, bool includeBenchmark)
    {
        if (!includeBenchmark)
        {
            return
            [
                FormatPlayer(r),
                r.GameCount,
                r.AverageBishopPairFrequency,
                r.MinPlyIndex,
                r.MaxPlyIndex
            ];
        }

        return
        [
            FormatPlayer(r),
            r.GameCount,
            r.AverageBishopPairFrequency,
            r.MinPlyIndex,
            r.MaxPlyIndex,
            r.CorpusAverage,
            r.DeltaFromCorpus,
            r.CorpusPercentile,
            r.CorpusEligiblePlayerCount
        ];
    }

    private static string FormatPlayer(BishopPairFrequencyRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
