namespace Interfaces.DTO;

/// <summary>
/// Secondary fact row for <c>dbo.GameMove</c> (one half-move / ply transition).
/// </summary>
public sealed class GameMoveFact
{
    public int GameId { get; set; }

    public int PlyIndex { get; set; }

    public char MovingSide { get; set; }

    public byte FromSquare { get; set; }

    public byte ToSquare { get; set; }

    public char MovedPiece { get; set; }

    public char? CapturedPiece { get; set; }

    public char? PromotionPiece { get; set; }

    public bool IsCastlingKingside { get; set; }

    public bool IsCastlingQueenside { get; set; }
}
