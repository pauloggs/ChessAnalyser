using Interfaces.Analytics;
using Interfaces.DTO;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Derives move facts and position summaries from the same ordered ply list used when persisting <c>BoardPosition</c>.
/// </summary>
public sealed class AnalyticsMaterializationService(
    IGameMoveDeriver moveDeriver,
    IGamePositionSummaryFactory summaryFactory,
    IChessRepository chessRepository) : IAnalyticsMaterializationService
{
    private readonly IGameMoveDeriver _moveDeriver = moveDeriver ?? throw new ArgumentNullException(nameof(moveDeriver));
    private readonly IGamePositionSummaryFactory _summaryFactory = summaryFactory ?? throw new ArgumentNullException(nameof(summaryFactory));
    private readonly IChessRepository _chessRepository = chessRepository ?? throw new ArgumentNullException(nameof(chessRepository));

    public async Task MaterializeAfterGamePersistedAsync(Game game, int databaseGameId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(game);
        var ordered = BuildOrderedPositions(game);
        await MaterializeFromOrderedPliesAsync(databaseGameId, ordered, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AnalyticsMaterializationOutcome> MaterializeFromOrderedPliesAsync(
        int databaseGameId,
        IReadOnlyList<(int PlyIndex, BoardPosition Position)> orderedPositions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderedPositions);
        if (orderedPositions.Count == 0)
            return AnalyticsMaterializationOutcome.SkippedNoPositions;

        try
        {
            var ordered = orderedPositions as List<(int PlyIndex, BoardPosition Position)>
                          ?? orderedPositions.ToList();

            for (var i = 1; i < ordered.Count; i++)
            {
                if (ordered[i].PlyIndex != ordered[i - 1].PlyIndex + 1)
                {
                    Console.WriteLine(
                        $"Analytics materialization skipped for game db id {databaseGameId}: non-contiguous ply sequence {ordered[i - 1].PlyIndex} -> {ordered[i].PlyIndex}.");
                    return AnalyticsMaterializationOutcome.SkippedInvalidSequence;
                }
            }

            var hasHalfMovePlies = ordered.Exists(p => p.PlyIndex >= 0);
            if (hasHalfMovePlies && ordered[0].PlyIndex != -1)
            {
                Console.WriteLine(
                    $"Analytics materialization skipped for game db id {databaseGameId}: ply -1 (initial position) is required when deriving moves.");
                return AnalyticsMaterializationOutcome.SkippedInvalidSequence;
            }

            var summaries = new List<GamePositionSummary>(ordered.Count);
            foreach (var (plyIndex, position) in ordered)
            {
                cancellationToken.ThrowIfCancellationRequested();
                summaries.Add(_summaryFactory.Create(databaseGameId, plyIndex, position));
            }

            var moves = new List<GameMoveFact>();
            for (var i = 0; i < ordered.Count - 1; i++)
            {
                var (_, previous) = ordered[i];
                var (nextPly, current) = ordered[i + 1];
                if (nextPly < 0)
                    continue;
                moves.Add(_moveDeriver.Derive(databaseGameId, nextPly, previous, current));
            }

            await _chessRepository.ReplaceGamePositionSummariesForGame(databaseGameId, summaries).ConfigureAwait(false);
            await _chessRepository.ReplaceGameMovesForGame(databaseGameId, moves).ConfigureAwait(false);
            return AnalyticsMaterializationOutcome.Success;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Analytics materialization failed for game db id {databaseGameId}: {ex.Message}");
            return AnalyticsMaterializationOutcome.Failed;
        }
    }

    /// <summary>
    /// Mirrors <see cref="ChessRepository.InsertBoardPositions"/> ordering so analytics align with persisted rows.
    /// </summary>
    private static List<(int PlyIndex, BoardPosition Position)> BuildOrderedPositions(Game game)
    {
        var positions = new List<(int PlyIndex, BoardPosition Position)>();

        if (game.BoardPositions != null)
        {
            foreach (var kvp in game.BoardPositions.OrderBy(k => k.Key))
                positions.Add((kvp.Key, kvp.Value));
        }

        if (game.InitialBoardPosition != null
            && (game.BoardPositions == null || !game.BoardPositions.ContainsKey(-1)))
            positions.Add((-1, game.InitialBoardPosition));

        return positions.OrderBy(x => x.PlyIndex).ToList();
    }
}
