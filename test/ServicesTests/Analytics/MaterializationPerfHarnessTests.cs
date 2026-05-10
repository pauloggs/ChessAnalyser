using Services.Analytics;

namespace ServicesTests.Analytics;

public class MaterializationPerfHarnessTests
{
    [Fact]
    public async Task RunAsync_SmallIterationCount_CompletesWithReasonableTiming()
    {
        var result = await MaterializationPerfHarness.RunAsync(20);

        Assert.Equal(20, result.Iterations);
        Assert.Equal(2, result.SummaryRowsPerIteration);
        Assert.Equal(1, result.MoveRowsPerIteration);
        Assert.True(result.ElapsedMilliseconds > 0);
        Assert.True(result.GamesPerSecond > 0);
    }
}
