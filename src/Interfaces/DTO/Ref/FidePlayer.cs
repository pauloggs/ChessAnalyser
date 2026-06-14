namespace Interfaces.DTO.Ref;

/// <summary>
/// FIDE rating-list player row from <c>Ref.FidePlayer</c> (migration <c>012</c>).
/// <see cref="Id"/> is the official FIDE player identifier (primary key).
/// </summary>
public sealed class FidePlayer
{
    public int Id { get; init; }

    public string Surname { get; init; } = string.Empty;

    public string Forenames { get; init; } = string.Empty;

    /// <summary>Three-letter federation code (e.g. NOR, IND).</summary>
    public string? Federation { get; init; }

    /// <summary>M or F when known.</summary>
    public string? Sex { get; init; }

    /// <summary>Normalised title: GM, IM, WGM, FM, WFM, CM, WCM.</summary>
    public string? FideTitle { get; init; }

    public short? BirthYear { get; init; }
}
