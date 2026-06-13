namespace Services.FideCatalog;

/// <summary>
/// Parses one row from the official FIDE combined rating-list TXT file (fixed-width).
/// Column layout matches the header line starting with <c>ID Number</c> (DESIGN §13.5).
/// </summary>
public interface IFideRatingListReader
{
    /// <summary>True for the column header line (starts with <c>ID Number</c>).</summary>
    bool IsHeaderLine(string line);

    /// <summary>True for data lines that can be parsed (numeric FIDE id at column 0).</summary>
    bool IsDataLine(string line);

    /// <summary>Parses a data line into a <see cref="FideCatalogRow"/>; returns null when the line is invalid.</summary>
    FideCatalogRow? ParseDataLine(string line);
}

/// <summary>One parsed FIDE list row before persistence to <c>Ref.FidePlayer</c>.</summary>
public sealed class FideCatalogRow
{
    public int Id { get; init; }

    public string Surname { get; init; } = string.Empty;

    public string Forenames { get; init; } = string.Empty;

    public string? Federation { get; init; }

    public string? Sex { get; init; }

    public string? FideTitle { get; init; }

    public short? BirthYear { get; init; }
}
