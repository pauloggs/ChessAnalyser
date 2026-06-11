using Interfaces.Analytics;

namespace Services.Analytics;

internal static class CorpusBenchmarkQueryDefaults
{
    public const int DefaultBenchmarkMinGames = 30;

    public static bool IncludeBenchmark(AnalyticsQuery query) =>
        query.IncludeCorpusBenchmark == true;

    public static int BenchmarkMinGames(AnalyticsQuery query) =>
        query.BenchmarkMinGames ?? DefaultBenchmarkMinGames;
}
