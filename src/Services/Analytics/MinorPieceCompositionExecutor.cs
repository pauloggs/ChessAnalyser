using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Mean bishops-minus-knights count for a player across a ply window (PLAN §12.6 Phase 2).
/// </summary>
public sealed class MinorPieceCompositionExecutor(
    IChessRepository repository,
    ICorpusBenchmarkCalculator corpusBenchmarkCalculator) : IMetricExecutor
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly ICorpusBenchmarkCalculator _corpusBenchmarkCalculator =
        corpusBenchmarkCalculator ?? throw new ArgumentNullException(nameof(corpusBenchmarkCalculator));

    public string MetricKey => "MinorPieceComposition";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        PlayerStyleMetricValidation.RequirePlayerFilter(query);
        var (minPly, maxPly) = PlayerStylePlyWindow.Resolve(query);
        var rows = (await _repository.GetMinorPieceCompositionAsync(query, minPly, maxPly, cancellationToken).ConfigureAwait(false)).ToList();

        if (CorpusBenchmarkQueryDefaults.IncludeCorpusBenchmark(query) && rows.Count > 0)
        {
            var perPlayer = await _repository
                .GetPerPlayerMinorPieceCompositionAsync(query, minPly, maxPly, cancellationToken)
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
                    "Player", "GameCount", "AverageMinorPieceDelta", "MinPlyIndex", "MaxPlyIndex",
                    "CorpusAverage", "DeltaFromCorpus", "CorpusPercentile", "CorpusEligiblePlayerCount"
                ]
                : ["Player", "GameCount", "AverageMinorPieceDelta", "MinPlyIndex", "MaxPlyIndex"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)FormatRow(r, includeBenchmark).ToList()).ToList()
        };
    }

    private MinorPieceCompositionRow EnrichWithBenchmark(
        MinorPieceCompositionRow row,
        AnalyticsQuery query,
        IReadOnlyList<PlayerStylePerPlayerMetricRow> perPlayer,
        int benchmarkMinGames)
    {
        var benchmark = _corpusBenchmarkCalculator.Compute(
            row.AverageMinorPieceDelta,
            query.PlayerSurname!,
            query.PlayerForenames,
            perPlayer,
            benchmarkMinGames);

        return new MinorPieceCompositionRow
        {
            PlayerSurname = row.PlayerSurname,
            PlayerForenames = row.PlayerForenames,
            GameCount = row.GameCount,
            AverageMinorPieceDelta = row.AverageMinorPieceDelta,
            MinPlyIndex = row.MinPlyIndex,
            MaxPlyIndex = row.MaxPlyIndex,
            CorpusAverage = benchmark.CorpusAverage,
            DeltaFromCorpus = benchmark.DeltaFromCorpus,
            CorpusPercentile = benchmark.CorpusPercentile,
            CorpusEligiblePlayerCount = benchmark.CorpusEligiblePlayerCount
        };
    }

    private static IReadOnlyList<object?> FormatRow(MinorPieceCompositionRow r, bool includeBenchmark)
    {
        if (!includeBenchmark)
        {
            return
            [
                FormatPlayer(r),
                r.GameCount,
                r.AverageMinorPieceDelta,
                r.MinPlyIndex,
                r.MaxPlyIndex
            ];
        }

        return
        [
            FormatPlayer(r),
            r.GameCount,
            r.AverageMinorPieceDelta,
            r.MinPlyIndex,
            r.MaxPlyIndex,
            r.CorpusAverage,
            r.DeltaFromCorpus,
            r.CorpusPercentile,
            r.CorpusEligiblePlayerCount
        ];
    }

    private static string FormatPlayer(MinorPieceCompositionRow row)
    {
        if (string.IsNullOrWhiteSpace(row.PlayerForenames))
            return row.PlayerSurname;

        return $"{row.PlayerSurname}, {row.PlayerForenames}";
    }
}
