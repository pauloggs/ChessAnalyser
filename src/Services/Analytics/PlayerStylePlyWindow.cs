using Interfaces.Analytics;

namespace Services.Analytics;

internal static class PlayerStylePlyWindow
{
    public const int DefaultMinPlyIndex = 15;

    public const int DefaultMaxPlyIndex = 30;

    /// <summary>
    /// Returns the ply window for position-based style metrics. When both bounds are unset, uses 15–30.
    /// </summary>
    public static (int? MinPlyIndex, int? MaxPlyIndex) Resolve(AnalyticsQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.MinPlyIndex is null && query.MaxPlyIndex is null)
            return (DefaultMinPlyIndex, DefaultMaxPlyIndex);

        return (query.MinPlyIndex, query.MaxPlyIndex);
    }
}
