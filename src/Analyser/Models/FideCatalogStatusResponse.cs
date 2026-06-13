namespace Analyser.Models;

public sealed class FideCatalogStatusResponse
{
    public int CatalogCount { get; set; }

    public string ConfiguredListPath { get; set; } = string.Empty;

    public bool ListFileExists { get; set; }
}
