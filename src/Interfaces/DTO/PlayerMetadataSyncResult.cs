namespace Interfaces.DTO;

/// <summary>
/// Outcome of synchronising curated player metadata (e.g. world-champion flag) onto dbo.Player rows.
/// </summary>
public sealed class PlayerMetadataSyncResult
{
    public int PlayersChecked { get; init; }

    public int PlayersUpdated { get; init; }
}
