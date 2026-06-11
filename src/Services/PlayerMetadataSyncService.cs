using Interfaces.DTO;
using Repositories;
using Services.PlayerMetadata;

namespace Services;

/// <inheritdoc />
public sealed class PlayerMetadataSyncService(
    IChessRepository repository,
    IWorldChampionMatcher worldChampionMatcher,
    IPlayerFideMetadataEnricher playerFideMetadataEnricher) : IPlayerMetadataSyncService
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IWorldChampionMatcher _worldChampionMatcher =
        worldChampionMatcher ?? throw new ArgumentNullException(nameof(worldChampionMatcher));
    private readonly IPlayerFideMetadataEnricher _playerFideMetadataEnricher =
        playerFideMetadataEnricher ?? throw new ArgumentNullException(nameof(playerFideMetadataEnricher));

    /// <inheritdoc />
    public async Task<PlayerMetadataSyncResult> SyncWorldChampionFlagsAsync(CancellationToken cancellationToken = default)
    {
        await _worldChampionMatcher.EnsureLoadedAsync(cancellationToken).ConfigureAwait(false);
        var players = await _repository.GetPlayers().ConfigureAwait(false);
        var updated = 0;

        foreach (var player in players)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var shouldBeChampion = _worldChampionMatcher.IsWorldChampion(player.Surname, player.Forenames);
            if (player.WasWorldChampion == shouldBeChampion)
                continue;

            await _repository.UpdatePlayerWasWorldChampionAsync(player.Id, shouldBeChampion, cancellationToken)
                .ConfigureAwait(false);
            updated++;
        }

        return new PlayerMetadataSyncResult
        {
            PlayersChecked = players.Count,
            PlayersUpdated = updated
        };
    }

    /// <inheritdoc />
    public Task<FideMetadataSyncResult> BackfillFideMetadataAsync(
        bool dryRun = false,
        CancellationToken cancellationToken = default) =>
        _playerFideMetadataEnricher.BackfillAllAsync(dryRun, cancellationToken);
}
