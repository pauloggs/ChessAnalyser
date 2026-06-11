namespace Interfaces.Analytics;

public sealed class FirstQueenMovePlyRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int GamesWithQueenMove { get; init; }

    public double? AverageFirstQueenMovePly { get; init; }

    public double? CorpusAverage { get; init; }

    public double? DeltaFromCorpus { get; init; }

    public double? CorpusPercentile { get; init; }

    public int? CorpusEligiblePlayerCount { get; init; }
}
