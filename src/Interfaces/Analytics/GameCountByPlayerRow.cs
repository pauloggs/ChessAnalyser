namespace Interfaces.Analytics;

public sealed class GameCountByPlayerRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string PlayerForenames { get; init; } = string.Empty;

    public int WhiteGameCount { get; init; }

    public int BlackGameCount { get; init; }

    public int TotalGameCount { get; init; }
}
