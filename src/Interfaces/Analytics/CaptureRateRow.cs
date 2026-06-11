namespace Interfaces.Analytics;

public sealed class CaptureRateRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int GameCount { get; init; }

    public double? AverageCaptureRate { get; init; }
}
