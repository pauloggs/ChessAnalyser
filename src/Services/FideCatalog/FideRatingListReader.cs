using Services.Helpers;

namespace Services.FideCatalog;

/// <inheritdoc />
public sealed class FideRatingListReader : IFideRatingListReader
{
    private const int MinLineLength = 156;
    private const int IdStart = 0;
    private const int IdLength = 15;
    private const int NameStart = 15;
    private const int NameLength = 60;
    private const int FedStart = 76;
    private const int FedLength = 3;
    private const int SexStart = 80;
    private const int SexLength = 1;
    private const int TitleStart = 84;
    private const int TitleLength = 3;
    private const int BirthYearStart = 152;
    private const int BirthYearLength = 4;

    private static readonly HashSet<string> KnownTitles = new(StringComparer.OrdinalIgnoreCase)
    {
        "GM", "IM", "FM", "CM", "WGM", "WIM", "WFM", "WCM"
    };

    /// <inheritdoc />
    public bool IsHeaderLine(string line) =>
        line.TrimStart().StartsWith("ID Number", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool IsDataLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line) || line.Length < IdLength)
            return false;

        var idText = Slice(line, IdStart, IdLength);
        return int.TryParse(idText, out var id) && id > 0;
    }

    /// <inheritdoc />
    public FideCatalogRow? ParseDataLine(string line)
    {
        if (!IsDataLine(line))
            return null;

        if (!int.TryParse(Slice(line, IdStart, IdLength), out var id) || id <= 0)
            return null;

        var name = Slice(line, NameStart, NameLength);
        if (string.IsNullOrWhiteSpace(name))
            return null;

        PlayerNameParser.Parse(name, out var surname, out var forenames);
        if (string.IsNullOrWhiteSpace(surname))
            return null;

        var federation = NormalizeOptional(Slice(line, FedStart, FedLength));
        var sex = NormalizeSex(Slice(line, SexStart, SexLength));
        var title = NormalizeTitle(Slice(line, TitleStart, TitleLength));
        var birthYear = ParseBirthYear(Slice(line, BirthYearStart, BirthYearLength));

        return new FideCatalogRow
        {
            Id = id,
            Surname = surname,
            Forenames = forenames ?? string.Empty,
            Federation = federation,
            Sex = sex,
            FideTitle = title,
            BirthYear = birthYear
        };
    }

    private static string Slice(string line, int start, int length)
    {
        if (start >= line.Length)
            return string.Empty;

        var available = Math.Min(length, line.Length - start);
        return line.Substring(start, available).Trim();
    }

    private static string? NormalizeOptional(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeSex(string value)
    {
        var s = value.Trim().ToUpperInvariant();
        return s is "M" or "F" ? s : null;
    }

    private static string? NormalizeTitle(string value)
    {
        var title = value.Trim().ToUpperInvariant();
        if (title.Length == 0)
            return null;

        if (KnownTitles.Contains(title))
            return title;

        if (title.Length >= 2 && KnownTitles.Contains(title[..2]))
            return title[..2];

        if (title.Length >= 3 && KnownTitles.Contains(title[..3]))
            return title[..3];

        return null;
    }

    private static short? ParseBirthYear(string value)
    {
        if (!short.TryParse(value.Trim(), out var year) || year < 1800 || year > 2100)
            return null;

        return year;
    }
}
