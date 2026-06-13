using Interfaces.DTO;

namespace Services.FideCatalog;

/// <summary>Streams a FIDE TXT file into <c>Ref.FidePlayer</c>.</summary>
public interface IFideCatalogSeedService
{
    /// <summary>
    /// Loads the catalog when <paramref name="force"/> is true or <c>Ref.FidePlayer</c> is empty.
    /// Clears <c>App.Player.FidePlayerId</c> before replacing catalog rows when <paramref name="force"/> is true.
    /// </summary>
    Task<FideCatalogSeedResult> SeedAsync(
        string? listPath = null,
        bool force = false,
        IProgress<MaintenanceProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
