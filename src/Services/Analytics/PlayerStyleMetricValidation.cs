using Interfaces.Analytics;

namespace Services.Analytics;

internal static class PlayerStyleMetricValidation
{
    public static void RequirePlayerFilter(AnalyticsQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (string.IsNullOrWhiteSpace(query.PlayerSurname))
            throw new ArgumentException("playerSurname is required for this style metric.", nameof(query));
    }
}
