namespace Interfaces.DTO.App;

/// <summary>
/// Chess player row from <c>App.Player</c>. Parsed from PGN White/Black tags.
/// </summary>
public class Player
{
    public int Id { get; set; }

    /// <summary>Surname (family name). No leading or trailing spaces.</summary>
    public string Surname { get; set; } = string.Empty;

    /// <summary>Forenames (given names). No leading or trailing spaces. May be empty.</summary>
    public string Forenames { get; set; } = string.Empty;

    /// <summary>FK to <c>Ref.WorldChampion</c> when matched; null otherwise.</summary>
    public int? WorldChampionId { get; set; }

    /// <summary>FK to <c>Ref.FidePlayer</c> when matched; null otherwise.</summary>
    public int? FidePlayerId { get; set; }
}
