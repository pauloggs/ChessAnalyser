namespace Interfaces.Analytics;

/// <summary>
/// Named material valuation policy for analytics rollups (DESIGN NFR-4).
/// Piece letters are <c>P N B R Q K</c> (type only, no colour).
/// </summary>
public interface IPieceValues
{
    /// <summary>Stable name for this profile (logged / future versioning).</summary>
    string ProfileName { get; }

    /// <summary>Returns the material value for a single piece of this type. King is typically 0.</summary>
    /// <param name="pieceType">'P','N','B','R','Q','K' (any casing).</param>
    short ValueForPieceType(char pieceType);
}
