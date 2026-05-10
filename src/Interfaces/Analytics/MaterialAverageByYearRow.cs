namespace Interfaces.Analytics;

/// <summary>
/// One grouped row for average white/black material by calendar year at a fixed ply.
/// </summary>
public sealed class MaterialAverageByYearRow
{
    public short GameYear { get; set; }

    public double AvgWhiteMaterial { get; set; }

    public double AvgBlackMaterial { get; set; }

    public int GameCount { get; set; }
}
