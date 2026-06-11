namespace Interfaces.DTO;

/// <summary>
/// A classical world-champion name alias in <c>Ref.WorldChampion</c>.
/// </summary>
public sealed class WorldChampionRef
{
    public int Id { get; set; }

    public string Surname { get; set; } = string.Empty;

    public string Forenames { get; set; } = string.Empty;

    public short ChampionOrder { get; set; }

    public short? ReignStartYear { get; set; }

    public short? ReignEndYear { get; set; }
}
