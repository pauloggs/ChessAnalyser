using System.Globalization;
using Interfaces.DTO;
using Services.Helpers;

namespace Services.PlayerMetadata;

/// <inheritdoc />
public sealed class FideRatingListReader : IFideRatingListReader
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<FidePlayerRecord>> ReadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"FIDE rating list file was not found: '{filePath}'. " +
                $"Working directory: '{Directory.GetCurrentDirectory()}'. " +
                "Download a TXT list from https://ratings.fide.com/download_lists.phtml and save it locally " +
                "(see data/fide/README.md). Use an absolute path if needed.",
                filePath);
        }

        var records = new List<FidePlayerRecord>();
        await foreach (var line in ReadLinesAsync(filePath, cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (TryParseLine(line, out var record))
                records.Add(record);
        }

        return records;
    }

    internal static bool TryParseLine(string line, out FidePlayerRecord record)
    {
        record = null!;
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("ID Number", StringComparison.OrdinalIgnoreCase))
            return false;

        if (line.Length < FideRatingListFieldLayout.MinLineLength)
            return false;

        var idText = Slice(line, FideRatingListFieldLayout.IdStart, FideRatingListFieldLayout.IdLength);
        if (!int.TryParse(idText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var fideId) || fideId <= 0)
            return false;

        var name = Slice(line, FideRatingListFieldLayout.NameStart, FideRatingListFieldLayout.NameLength);
        if (string.IsNullOrWhiteSpace(name))
            return false;

        PlayerNameParser.Parse(name, out var surname, out var forenames);

        var federation = NullIfEmpty(Slice(line, FideRatingListFieldLayout.FederationStart, FideRatingListFieldLayout.FederationLength));
        var sex = NullIfEmpty(Slice(line, FideRatingListFieldLayout.SexStart, FideRatingListFieldLayout.SexLength));
        var titleRegion = Slice(line, FideRatingListFieldLayout.TitleRegionStart, FideRatingListFieldLayout.TitleRegionLength);
        var title = FideTitleNormalizer.Normalize(titleRegion);
        var birthYear = TryParseBirthYear(line);

        record = new FidePlayerRecord
        {
            FideId = fideId,
            Name = name,
            Surname = surname,
            Forenames = forenames,
            Federation = federation,
            Sex = sex,
            Title = title,
            BirthYear = birthYear
        };
        return true;
    }

    private static short? TryParseBirthYear(string line)
    {
        var parts = line.TrimEnd().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = parts.Length - 1; i >= 0; i--)
        {
            var part = parts[i];
            if (part is "i" or "wi" or "w")
                continue;

            if (part.Length == 4 &&
                short.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var year) &&
                year is >= 1800 and <= 2100)
                return year;

            break;
        }

        return null;
    }

    private static string Slice(string line, int start, int length)
    {
        if (start >= line.Length)
            return string.Empty;

        var take = Math.Min(length, line.Length - start);
        return line.Substring(start, take).Trim();
    }

    private static string? NullIfEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static async IAsyncEnumerable<string> ReadLinesAsync(
        string filePath,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(filePath);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null)
                yield break;
            yield return line;
        }
    }
}
