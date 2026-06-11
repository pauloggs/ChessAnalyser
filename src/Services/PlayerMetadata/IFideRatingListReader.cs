using Interfaces.DTO;

namespace Services.PlayerMetadata;

/// <summary>Reads official FIDE rating list TXT files (fixed-width format).</summary>
public interface IFideRatingListReader
{
    /// <summary>Parses all player rows from a FIDE TXT list file.</summary>
    Task<IReadOnlyList<FidePlayerRecord>> ReadAsync(string filePath, CancellationToken cancellationToken = default);
}
