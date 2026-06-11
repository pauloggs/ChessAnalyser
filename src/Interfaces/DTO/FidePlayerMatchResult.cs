namespace Interfaces.DTO;

public enum FidePlayerMatchOutcome
{
    Matched,
    Unmatched,
    Ambiguous
}

/// <summary>Outcome of matching a corpus <see cref="Player"/> name to a FIDE list row.</summary>
public sealed class FidePlayerMatchResult
{
    public FidePlayerMatchOutcome Outcome { get; init; }

    public FidePlayerRecord? Record { get; init; }
}
