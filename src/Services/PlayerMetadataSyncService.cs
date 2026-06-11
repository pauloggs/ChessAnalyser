using Interfaces.DTO;
using Repositories;
using Services.PlayerMetadata;

namespace Services;

/// <inheritdoc />
public sealed class PlayerMetadataSyncService(IChessRepository repository) : IPlayerMetadataSyncService
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    /// <inheritdoc />
    public async Task<PlayerMetadataSyncResult> SyncWorldChampionFlagsAsync(CancellationToken cancellationToken = default)
    {
        var players = await _repository.GetPlayers().ConfigureAwait(false);
        var updated = 0;

        foreach (var player in players)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var shouldBeChampion = WorldChampionCatalog.IsWorldChampion(player.Surname, player.Forenames);
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
}
