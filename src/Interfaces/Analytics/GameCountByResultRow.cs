namespace Interfaces.Analytics;

public sealed class GameCountByResultRow
{
    public string Result { get; init; } = string.Empty;

    public int GameCount { get; init; }
}
