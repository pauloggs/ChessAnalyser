namespace Interfaces.DTO;

/// <summary>
/// Outcome of linking <c>App.Player</c> rows to <c>Ref.WorldChampion</c> / <c>Ref.FidePlayer</c>.
/// </summary>
public sealed class PlayerMetadataLinkResult
{
    public int PlayersProcessed { get; init; }

    public int WorldChampionLinked { get; init; }

    public int FideLinked { get; init; }

    public int FideCleared { get; init; }

    public int Unchanged { get; init; }

    public static PlayerMetadataLinkResult Empty { get; } = new();

    public PlayerMetadataLinkResult WithSinglePlayer(
        bool worldChampionChanged,
        bool fideLinked,
        bool fideCleared,
        bool unchanged)
    {
        return new PlayerMetadataLinkResult
        {
            PlayersProcessed = 1,
            WorldChampionLinked = worldChampionChanged ? 1 : 0,
            FideLinked = fideLinked ? 1 : 0,
            FideCleared = fideCleared ? 1 : 0,
            Unchanged = unchanged ? 1 : 0
        };
    }

    public PlayerMetadataLinkResult Add(PlayerMetadataLinkResult other) =>
        new()
        {
            PlayersProcessed = PlayersProcessed + other.PlayersProcessed,
            WorldChampionLinked = WorldChampionLinked + other.WorldChampionLinked,
            FideLinked = FideLinked + other.FideLinked,
            FideCleared = FideCleared + other.FideCleared,
            Unchanged = Unchanged + other.Unchanged
        };
}
