using Interfaces.DTO;

namespace Interfaces.Analytics;

/// <summary>
/// Builds and persists <see cref="GameMoveFact"/> and <see cref="GamePositionSummary"/> rows
/// from in-memory <see cref="Game.BoardPositions"/> after the game and board snapshots are stored.
/// </summary>
public interface IAnalyticsMaterializationService
{
    /// <summary>
    /// Materializes analytics for a game that has already been inserted (including <c>BoardPosition</c> rows).
    /// Failures are logged and swallowed so ETL can continue; cancellation is propagated.
    /// </summary>
    Task MaterializeAfterGamePersistedAsync(Game game, int databaseGameId, CancellationToken cancellationToken = default);
}
