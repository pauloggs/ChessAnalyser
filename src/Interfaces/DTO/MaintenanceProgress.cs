namespace Interfaces.DTO;

/// <summary>Progress for long-running maintenance jobs (FIDE catalog seed, player metadata link).</summary>
public sealed class MaintenanceProgress
{
    /// <summary>e.g. SeedFideCatalog | LinkPlayerMetadata</summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>Running | Completed | Failed | Skipped</summary>
    public string Status { get; set; } = "Running";

    public string? Message { get; set; }

    public int? PercentComplete { get; set; }

    public int RowsProcessed { get; set; }

    public int? TotalRows { get; set; }
}
