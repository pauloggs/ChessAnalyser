namespace Analyser;

/// <summary>
/// Options for PGN loading, bound from configuration (e.g. appsettings.json "Pgn" section).
/// </summary>
public class PgnOptions
{
    public const string SectionName = "Pgn";

    /// <summary>
    /// Default file or directory path from which to load PGN files when none is specified.
    /// </summary>
    public string DefaultFilePath { get; set; } = "C:\\Library\\PGN";
}
