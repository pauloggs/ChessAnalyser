namespace Interfaces.Analytics;

public sealed class AverageMaterialVolatilityRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int GameCount { get; init; }

    public double? AverageMaterialVolatility { get; init; }

    public double? CorpusAverage { get; init; }

    public double? DeltaFromCorpus { get; init; }

    public double? CorpusPercentile { get; init; }

    public int? CorpusEligiblePlayerCount { get; init; }
}
