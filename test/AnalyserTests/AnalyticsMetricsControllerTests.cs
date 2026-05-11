using Analyser.Controllers;
using Analyser.Models;
using Interfaces.Analytics;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ControllerTests;

public class AnalyticsMetricsControllerTests
{
    [Fact]
    public void GetMetrics_ReturnsOrderedDescriptors()
    {
        var registry = new Mock<IMetricRegistry>();
        registry.Setup(r => r.MetricKeys).Returns(new[] { "KnightMoveDestinationFrequency", "AverageMaterialByYearAndColour" });

        var sut = new AnalyticsMetricsController(registry.Object);
        var result = sut.GetMetrics();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IReadOnlyList<MetricDescriptorResponse>>(ok.Value);
        Assert.Equal(2, list.Count);
        Assert.Equal("AverageMaterialByYearAndColour", list[0].MetricKey);
        Assert.Equal("KnightMoveDestinationFrequency", list[1].MetricKey);
        Assert.False(string.IsNullOrEmpty(list[0].Description));
    }

    [Fact]
    public async Task Execute_UnknownMetric_Returns404()
    {
        var registry = new Mock<IMetricRegistry>();
        registry.Setup(r => r.ExecuteAsync("nope", It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        var sut = new AnalyticsMetricsController(registry.Object);
        var result = await sut.Execute(new ExecuteMetricRequest { MetricKey = "nope", Query = null }, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Execute_InvalidMetricArguments_Returns400()
    {
        var registry = new Mock<IMetricRegistry>();
        registry.Setup(r => r.ExecuteAsync("AverageMaterialByPlayerAtMove", It.IsAny<AnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("playerASurname is required."));

        var sut = new AnalyticsMetricsController(registry.Object);
        var result = await sut.Execute(
            new ExecuteMetricRequest { MetricKey = "AverageMaterialByPlayerAtMove", Query = new AnalyticsQuery() },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("playerASurname is required.", badRequest.Value);
    }

    [Fact]
    public async Task Execute_ValidMetric_ReturnsTable()
    {
        var table = new AnalyticsTableResult
        {
            ColumnNames = new[] { "GameYear", "AvgWhiteMaterial" },
            Rows = new IReadOnlyList<object?>[] { new object?[] { (short)2000, 39.5 } }
        };

        var registry = new Mock<IMetricRegistry>();
        registry.Setup(r => r.ExecuteAsync(
                "AverageMaterialByYearAndColour",
                It.Is<AnalyticsQuery>(q => q.MinGameYear == 1999 && q.Eco == "C00"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(table);

        var sut = new AnalyticsMetricsController(registry.Object);
        var result = await sut.Execute(
            new ExecuteMetricRequest
            {
                MetricKey = "AverageMaterialByYearAndColour",
                Query = new AnalyticsQuery { MinGameYear = 1999, Eco = "C00" }
            },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<AnalyticsTableResponse>(ok.Value);
        Assert.Equal(new[] { "GameYear", "AvgWhiteMaterial" }, body.ColumnNames);
        Assert.Single(body.Rows);
        Assert.Equal((short)2000, body.Rows[0][0]);
        Assert.Equal(39.5, body.Rows[0][1]);
    }

    [Fact]
    public async Task Execute_MissingBody_Returns400()
    {
        var registry = new Mock<IMetricRegistry>();
        var sut = new AnalyticsMetricsController(registry.Object);
        var result = await sut.Execute(null, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Execute_EmptyMetricKey_Returns400()
    {
        var registry = new Mock<IMetricRegistry>();
        var sut = new AnalyticsMetricsController(registry.Object);
        var result = await sut.Execute(new ExecuteMetricRequest { MetricKey = "  ", Query = null }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
