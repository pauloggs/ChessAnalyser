namespace Interfaces.DTO;

/// <summary>
/// Optional filters for paged game listing. When multiple properties are set, they combine with AND.
/// </summary>
public sealed class GamePageFilters
{
    public short? MinGameYear { get; init; }

    public short? MaxGameYear { get; init; }

    public string? WhitePlayerSurname { get; init; }

    public string? WhitePlayerForenames { get; init; }

    public string? BlackPlayerSurname { get; init; }

    public string? BlackPlayerForenames { get; init; }

    /// <summary>Exact match on persisted <c>Eco</c> (max 16 characters in DB).</summary>
    public string? Eco { get; init; }
}
