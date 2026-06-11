namespace Services.PlayerMetadata;

/// <summary>
/// Fixed-width column slices for FIDE rating list TXT files (standard and combined STD/RPD/BLZ).
/// Verified against <c>standard_rating_list.txt</c> and <c>players_list_foa.txt</c> from ratings.fide.com.
/// </summary>
internal static class FideRatingListFieldLayout
{
    internal const int MinLineLength = 130;

    internal const int IdStart = 0;
    internal const int IdLength = 15;

    internal const int NameStart = 15;
    internal const int NameLength = 60;

    internal const int FederationStart = 76;
    internal const int FederationLength = 3;

    internal const int SexStart = 80;
    internal const int SexLength = 1;

    /// <summary>Tit + WTit region (may contain spaces; normalised by <see cref="FideTitleNormalizer"/>).</summary>
    internal const int TitleRegionStart = 81;
    internal const int TitleRegionLength = 14;
}
