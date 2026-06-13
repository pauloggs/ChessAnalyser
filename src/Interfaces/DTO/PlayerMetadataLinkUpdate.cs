namespace Interfaces.DTO;

/// <summary>Row for bulk <c>App.Player</c> metadata FK updates.</summary>
public sealed class PlayerMetadataLinkUpdate
{
    public int Id { get; init; }

    public int? WorldChampionId { get; init; }

    public int? FidePlayerId { get; init; }
}
