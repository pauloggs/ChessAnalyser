namespace Interfaces.DTO;

/// <summary>
/// One player row parsed from an official FIDE rating list TXT file.
/// </summary>
public sealed class FidePlayerRecord
{
    public int FideId { get; init; }

    /// <summary>Full name as stored by FIDE (usually <c>Surname, Forenames</c>).</summary>
    public string Name { get; init; } = string.Empty;

    public string Surname { get; init; } = string.Empty;

    public string Forenames { get; init; } = string.Empty;

    public string? Federation { get; init; }

    public string? Sex { get; init; }

    /// <summary>Normalised title: GM, IM, WGM, FM, WFM, CM, WCM; null when untitled.</summary>
    public string? Title { get; init; }

    public short? BirthYear { get; init; }
}
