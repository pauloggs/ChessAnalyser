using Interfaces.Analytics;

namespace Services.Analytics;

/// <summary>
/// Classical material: P=1, N=3, B=3, R=5, Q=9, K=0 (kings excluded from material totals).
/// </summary>
public sealed class ClassicalPieceValues : IPieceValues
{
    public string ProfileName => "classical-1-3-3-5-9-0";

    public short ValueForPieceType(char pieceType)
    {
        return char.ToUpperInvariant(pieceType) switch
        {
            'P' => 1,
            'N' => 3,
            'B' => 3,
            'R' => 5,
            'Q' => 9,
            'K' => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(pieceType), pieceType,
                "Expected P, N, B, R, Q, or K.")
        };
    }
}
