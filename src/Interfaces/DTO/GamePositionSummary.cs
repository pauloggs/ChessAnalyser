namespace Interfaces.DTO;

/// <summary>
/// Rollup row for <c>dbo.GamePositionSummary</c> (material + per-type counts at a ply).
/// </summary>
public sealed class GamePositionSummary
{
    public int GameId { get; set; }

    public int PlyIndex { get; set; }

    public short WhiteMaterial { get; set; }

    public short BlackMaterial { get; set; }

    public short WhitePawnCount { get; set; }

    public short WhiteKnightCount { get; set; }

    public short WhiteBishopCount { get; set; }

    public short WhiteRookCount { get; set; }

    public short WhiteQueenCount { get; set; }

    public short WhiteKingCount { get; set; }

    public short BlackPawnCount { get; set; }

    public short BlackKnightCount { get; set; }

    public short BlackBishopCount { get; set; }

    public short BlackRookCount { get; set; }

    public short BlackQueenCount { get; set; }

    public short BlackKingCount { get; set; }
}
