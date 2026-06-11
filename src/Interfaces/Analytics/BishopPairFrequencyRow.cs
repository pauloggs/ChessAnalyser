namespace Interfaces.Analytics;

public sealed class BishopPairFrequencyRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int GameCount { get; init; }

    public double? AverageBishopPairFrequency { get; init; }

    public int? MinPlyIndex { get; init; }

    public int? MaxPlyIndex { get; init; }

    public double? CorpusAverage { get; init; }

    public double? DeltaFromCorpus { get; init; }

    public double? CorpusPercentile { get; init; }

    public int? CorpusEligiblePlayerCount { get; init; }
}
