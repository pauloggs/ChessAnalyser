namespace Interfaces.DTO;

/// <summary>First and last calendar year a player appears in the loaded game corpus (nullable years excluded).</summary>
public sealed class PlayerCorpusActivity
{
    public short? FirstGameYear { get; init; }

    public short? LastGameYear { get; init; }
}
