using Interfaces.DTO;

namespace Interfaces.Analytics;

/// <summary>
/// Derives a <see cref="GameMoveFact"/> from consecutive board positions.
/// </summary>
public interface IGameMoveDeriver
{
    GameMoveFact Derive(int gameId, int plyIndex, BoardPosition previous, BoardPosition current);
}
