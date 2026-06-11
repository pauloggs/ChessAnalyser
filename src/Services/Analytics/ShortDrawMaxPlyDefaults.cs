using Interfaces.Analytics;

namespace Services.Analytics;

internal static class ShortDrawMaxPlyDefaults
{
    public const int DefaultShortDrawMaxPly = 20;

    public static int Resolve(AnalyticsQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return query.ShortDrawMaxPly ?? DefaultShortDrawMaxPly;
    }
}
