namespace Interfaces.DTO;

/// <summary>
/// Outcome of synchronising FIDE list metadata onto <see cref="Player"/> rows.
/// </summary>
public sealed class FideMetadataSyncResult
{
    public int PlayersChecked { get; init; }

    public int PlayersUpdated { get; init; }

    public int PlayersMatched { get; init; }

    public int PlayersUnmatched { get; init; }

    public int PlayersAmbiguous { get; init; }

    /// <summary>Matched players skipped because another corpus row already owns the FIDE ID.</summary>
    public int PlayersFideIdConflict { get; init; }

    public bool DryRun { get; init; }
}
