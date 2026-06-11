namespace Interfaces.Analytics;

public sealed class OppositeSideCastlingRateRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int EligibleGameCount { get; init; }

    public double? OppositeSideCastlingRate { get; init; }

    public double? CorpusAverage { get; init; }

    public double? DeltaFromCorpus { get; init; }

    public double? CorpusPercentile { get; init; }

    public int? CorpusEligiblePlayerCount { get; init; }
}
