namespace Interfaces.DTO.Ref;

/// <summary>
/// Canonical classical world-champion row from <c>Ref.WorldChampion</c> (migration <c>011</c>).
/// </summary>
public sealed class WorldChampion
{
    public int Id { get; init; }

    public string Surname { get; init; } = string.Empty;

    public string Forenames { get; init; } = string.Empty;

    public short ChampionOrder { get; init; }

    public short? ReignStartYear { get; init; }

    public short? ReignEndYear { get; init; }
}
