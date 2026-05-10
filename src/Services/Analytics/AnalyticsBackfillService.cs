using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

public sealed class AnalyticsBackfillService(
    IChessRepository chessRepository,
    IAnalyticsMaterializationService materialization) : IAnalyticsBackfillService
{
    private readonly IChessRepository _chessRepository = chessRepository ?? throw new ArgumentNullException(nameof(chessRepository));
    private readonly IAnalyticsMaterializationService _materialization = materialization ?? throw new ArgumentNullException(nameof(materialization));

    public async Task<AnalyticsBackfillResult> BackfillMissingAnalyticsAsync(
        AnalyticsBackfillOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new AnalyticsBackfillOptions();
        var ids = await _chessRepository.GetGameIdsNeedingAnalyticsBackfillAsync(cancellationToken).ConfigureAwait(false);
        var slice = options.MaxGames is { } cap ? ids.Take(cap).ToList() : ids.ToList();

        var materialized = 0;
        var skipped = 0;
        var failed = 0;

        foreach (var gameId in slice)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var positions = await _chessRepository.GetBoardPositionsForGameOrderedAsync(gameId, cancellationToken).ConfigureAwait(false);
            var ordered = positions.ToList();
            var outcome = await _materialization.MaterializeFromOrderedPliesAsync(gameId, ordered, cancellationToken).ConfigureAwait(false);
            switch (outcome)
            {
                case AnalyticsMaterializationOutcome.Success:
                    materialized++;
                    break;
                case AnalyticsMaterializationOutcome.SkippedNoPositions:
                case AnalyticsMaterializationOutcome.SkippedInvalidSequence:
                    skipped++;
                    break;
                case AnalyticsMaterializationOutcome.Failed:
                    failed++;
                    break;
            }
        }

        return new AnalyticsBackfillResult
        {
            GamesConsidered = slice.Count,
            GamesMaterialized = materialized,
            GamesSkipped = skipped,
            GamesFailed = failed
        };
    }
}
