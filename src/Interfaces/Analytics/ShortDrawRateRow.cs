namespace Interfaces.Analytics;

public sealed class ShortDrawRateRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int DrawCount { get; init; }

    public double? ShortDrawRate { get; init; }

    public int ShortDrawMaxPly { get; init; }

    public double? CorpusAverage { get; init; }

    public double? DeltaFromCorpus { get; init; }

    public double? CorpusPercentile { get; init; }

    public int? CorpusEligiblePlayerCount { get; init; }
}
