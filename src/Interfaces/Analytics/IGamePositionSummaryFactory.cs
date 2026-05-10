using Interfaces.DTO;

namespace Interfaces.Analytics;

/// <summary>
/// Builds <see cref="GamePositionSummary"/> rows from a <see cref="BoardPosition"/> bitboard snapshot (PLAN §5.3.3).
/// </summary>
public interface IGamePositionSummaryFactory
{
    GamePositionSummary Create(int gameId, int plyIndex, BoardPosition boardPosition);
}
