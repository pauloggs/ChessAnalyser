using Interfaces.DTO;

namespace Services.PlayerMetadata;

/// <summary>Matches corpus player names to rows from a FIDE rating list.</summary>
public interface IFidePlayerMatcher
{
    /// <summary>Indexes FIDE rows for matching. Call once per list load.</summary>
    void SetRecords(IReadOnlyList<FidePlayerRecord> records);

    /// <summary>
    /// Finds a FIDE row for the given surname and forenames using <see cref="Services.Helpers.PlayerForenamesMatcher"/>.
    /// </summary>
    FidePlayerMatchResult Match(string surname, string? forenames, FidePlayerMatchContext? context = null);
}
