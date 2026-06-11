namespace Interfaces.DTO;

/// <summary>Optional disambiguation hints when matching a corpus player to FIDE rows (DESIGN §13.5).</summary>
public sealed class FidePlayerMatchContext
{
    public short? KnownBirthYear { get; init; }

    public short? CorpusFirstGameYear { get; init; }

    public short? CorpusLastGameYear { get; init; }
}
