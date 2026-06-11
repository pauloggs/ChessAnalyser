using Interfaces.DTO;

namespace Services.PlayerMetadata;

/// <summary>Matches corpus player names to rows from <c>Ref.FidePlayer</c>.</summary>
public interface IFidePlayerMatcher
{
    /// <summary>Loads catalog rows from <c>Ref.FidePlayer</c> when not already cached in this scope.</summary>
    Task EnsureLoadedAsync(CancellationToken cancellationToken = default);

    /// <summary>Clears the in-memory catalog so the next match reloads from the database.</summary>
    void InvalidateCache();

    /// <summary>Loads an in-memory catalog snapshot without persisting (e.g. dry-run import preview).</summary>
    void LoadCatalogSnapshot(IReadOnlyList<FidePlayerRecord> records);

    /// <summary>
    /// Finds a FIDE row for the given surname and forenames using <see cref="Services.Helpers.PlayerForenamesMatcher"/>.
    /// </summary>
    FidePlayerMatchResult Match(string surname, string? forenames, FidePlayerMatchContext? context = null);
}
