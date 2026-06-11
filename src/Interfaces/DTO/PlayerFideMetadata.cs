namespace Interfaces.DTO;

/// <summary>
/// FIDE-sourced attributes for a <see cref="Player"/> row (DESIGN §13.3).
/// </summary>
public sealed class PlayerFideMetadata
{
    public int? FideId { get; init; }

    /// <summary>Three-letter federation code (e.g. NOR, IND).</summary>
    public string? Federation { get; init; }

    /// <summary>M or F when known.</summary>
    public string? Sex { get; init; }

    /// <summary>Normalised title: GM, IM, WGM, FM, WFM, CM, WCM.</summary>
    public string? FideTitle { get; init; }

    public short? BirthYear { get; init; }
}
