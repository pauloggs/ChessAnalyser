using Interfaces.DTO;
using Repositories;
using Services.Helpers;

namespace Services.PlayerMetadata;

/// <inheritdoc />
public sealed class WorldChampionMatcher(IChessRepository repository) : IWorldChampionMatcher
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private IReadOnlyList<WorldChampionRef>? _champions;

    /// <inheritdoc />
    public async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
    {
        if (_champions != null)
            return;

        _champions = await _repository.GetWorldChampions(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public bool IsWorldChampion(string surname, string? forenames)
    {
        if (_champions == null || _champions.Count == 0 || string.IsNullOrWhiteSpace(surname))
            return false;

        var normalizedForenames = (forenames ?? string.Empty).Trim();
        foreach (var champion in _champions)
        {
            if (!string.Equals(champion.Surname, surname.Trim(), StringComparison.OrdinalIgnoreCase))
                continue;

            if (PlayerForenamesMatcher.ForenamesMatch(champion.Forenames, normalizedForenames))
                return true;
        }

        return false;
    }
}
