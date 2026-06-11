namespace Interfaces.Analytics;

public sealed class CastlingSidePreferenceRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int GamesWithCastling { get; init; }

    public double? KingsideRate { get; init; }

    public double? QueensideRate { get; init; }

    public double? CorpusAverage { get; init; }

    public double? DeltaFromCorpus { get; init; }

    public double? CorpusPercentile { get; init; }

    public int? CorpusEligiblePlayerCount { get; init; }
}
