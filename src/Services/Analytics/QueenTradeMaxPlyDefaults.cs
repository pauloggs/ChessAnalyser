using Interfaces.Analytics;

namespace Services.Analytics;

internal static class QueenTradeMaxPlyDefaults
{
    public const int DefaultQueenTradeMaxPly = 40;

    public static int Resolve(AnalyticsQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return query.QueenTradeMaxPly ?? DefaultQueenTradeMaxPly;
    }
}
