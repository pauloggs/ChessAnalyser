using Analyser.Models;
using Interfaces.Analytics;
using Microsoft.AspNetCore.Mvc;

namespace Analyser.Controllers;

/// <summary>
/// HTTP surface for registered analytics metrics (<see cref="IMetricRegistry"/>). **No authentication** while the host is local-only; add auth / rate limits before network deployment (see PLAN §12.1, §12.4, §13).
/// </summary>
[ApiController]
[Route("api/analytics/metrics")]
public sealed class AnalyticsMetricsController(IMetricRegistry metricRegistry) : ControllerBase
{
    private readonly IMetricRegistry _metricRegistry = metricRegistry ?? throw new ArgumentNullException(nameof(metricRegistry));

    /// <summary>
    /// Lists metric keys registered at startup plus optional descriptions.
    /// </summary>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IReadOnlyList<MetricDescriptorResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<MetricDescriptorResponse>> GetMetrics()
    {
        var list = _metricRegistry.MetricKeys
            .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
            .Select(k => new MetricDescriptorResponse
            {
                MetricKey = k,
                Description = MetricCatalog.TryGetDescription(k)
            })
            .ToList();

        return Ok(list);
    }

    /// <summary>
    /// Runs a single metric with optional <see cref="AnalyticsQuery"/> filters.
    /// </summary>
    [HttpPost("execute")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AnalyticsTableResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnalyticsTableResponse>> Execute(
        [FromBody] ExecuteMetricRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.MetricKey))
            return BadRequest("metricKey is required.");

        var query = request.Query ?? new AnalyticsQuery();

        try
        {
            var table = await _metricRegistry
                .ExecuteAsync(request.MetricKey.Trim(), query, cancellationToken)
                .ConfigureAwait(false);
            return Ok(AnalyticsTableResponse.FromResult(table));
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Unknown metric key: {request.MetricKey.Trim()}");
        }
    }
}
