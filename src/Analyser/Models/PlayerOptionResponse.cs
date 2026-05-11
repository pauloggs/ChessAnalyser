namespace Analyser.Models;

/// <summary>
/// Lightweight player option used by the local web UI filters.
/// </summary>
public sealed class PlayerOptionResponse
{
    public int Id { get; init; }

    public required string DisplayName { get; init; }
}
