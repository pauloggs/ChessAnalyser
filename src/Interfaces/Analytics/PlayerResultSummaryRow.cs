namespace Interfaces.Analytics;

public sealed class PlayerResultSummaryRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string PlayerForenames { get; init; } = string.Empty;

    public int WinCount { get; init; }

    public int LossCount { get; init; }

    public int DrawCount { get; init; }

    public int UnknownCount { get; init; }

    public int TotalGameCount { get; init; }

    public double Score { get; init; }
}
