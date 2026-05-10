namespace Interfaces.DTO;

/// <summary>
/// One page of results plus paging metadata for API responses.
/// </summary>
public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>1-based page index.</summary>
    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }
}
