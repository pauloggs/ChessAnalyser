using Interfaces.DTO.Ref;

namespace Services.PlayerMetadata;

/// <summary>
/// Matches PGN player names to <c>Ref.FidePlayer</c> rows (DESIGN §13.5).
/// </summary>
public interface IFidePlayerMatcher
{
    /// <summary>
    /// Returns the FIDE catalog <see cref="FidePlayer.Id"/> when uniquely matched; otherwise null.
    /// </summary>
    int? Match(
        string surname,
        string forenames,
        IReadOnlyList<FidePlayer> candidates,
        short? referenceGameYear);
}
