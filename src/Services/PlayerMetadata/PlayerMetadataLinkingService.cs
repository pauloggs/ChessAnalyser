using Interfaces.DTO;
using Interfaces.DTO.Ref;
using Repositories;

namespace Services.PlayerMetadata;

/// <inheritdoc />
public sealed class PlayerMetadataLinkingService(
    IChessRepository repository,
    IWorldChampionMatcher worldChampionMatcher,
    IFidePlayerMatcher fidePlayerMatcher) : IPlayerMetadataLinkingService
{
    private const int UpdateBatchSize = 1000;
    private const int ProgressInterval = 100;

    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IWorldChampionMatcher _worldChampionMatcher =
        worldChampionMatcher ?? throw new ArgumentNullException(nameof(worldChampionMatcher));
    private readonly IFidePlayerMatcher _fidePlayerMatcher =
        fidePlayerMatcher ?? throw new ArgumentNullException(nameof(fidePlayerMatcher));

    private IReadOnlyList<WorldChampion>? _worldChampions;

    /// <inheritdoc />
    public async Task<PlayerMetadataLinkResult> TryLinkPlayerAsync(
        int playerId,
        short? gameYear = null,
        CancellationToken cancellationToken = default)
    {
        var player = await _repository.GetPlayerByIdAsync(playerId, cancellationToken);
        if (player is null)
            return PlayerMetadataLinkResult.Empty;

        _worldChampions ??= await _repository.GetWorldChampionsAsync(cancellationToken);
        var referenceGameYear = gameYear ?? await _repository.GetMinGameYearForPlayerAsync(playerId, cancellationToken);
        var fideCandidates = await _repository.GetFidePlayersBySurnameAsync(player.Surname, cancellationToken);

        var outcome = ComputeOutcome(player, referenceGameYear, fideCandidates, _worldChampions);
        if (outcome.ShouldUpdate)
        {
            await _repository.UpdatePlayerMetadataLinksAsync(
                playerId,
                outcome.WorldChampionId,
                outcome.FidePlayerId,
                cancellationToken);
        }

        return outcome.Result;
    }

    /// <inheritdoc />
    public async Task<PlayerMetadataLinkResult> LinkAllPlayersAsync(
        IProgress<MaintenanceProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        Report(progress, "Running", 0, "Loading reference data…", 0);

        _worldChampions = await _repository.GetWorldChampionsAsync(cancellationToken);
        var players = await _repository.GetPlayers();
        var minGameYears = await _repository.GetAllPlayerMinGameYearsAsync(cancellationToken);
        var fideCatalog = await _repository.GetFidePlayersForDistinctPlayerSurnamesAsync(cancellationToken);
        var fideBySurname = BuildFideSurnameIndex(fideCatalog);

        var total = players.Count;
        var processed = 0;
        var aggregate = PlayerMetadataLinkResult.Empty;
        var pendingUpdates = new List<PlayerMetadataLinkUpdate>(capacity: Math.Min(total, UpdateBatchSize));

        Report(progress, "Running", 0, $"Linking 0 / {total:N0} players…", 0);

        foreach (var player in players)
        {
            cancellationToken.ThrowIfCancellationRequested();

            minGameYears.TryGetValue(player.Id, out var referenceGameYear);
            fideBySurname.TryGetValue(player.Surname, out var fideCandidates);
            fideCandidates ??= [];

            var outcome = ComputeOutcome(player, referenceGameYear, fideCandidates, _worldChampions);
            aggregate = aggregate.Add(outcome.Result);

            if (outcome.ShouldUpdate)
            {
                pendingUpdates.Add(new PlayerMetadataLinkUpdate
                {
                    Id = player.Id,
                    WorldChampionId = outcome.WorldChampionId,
                    FidePlayerId = outcome.FidePlayerId
                });

                if (pendingUpdates.Count >= UpdateBatchSize)
                {
                    await _repository.BulkUpdatePlayerMetadataLinksAsync(pendingUpdates, cancellationToken);
                    pendingUpdates.Clear();
                }
            }

            processed++;
            if (processed == total || processed % ProgressInterval == 0)
            {
                var percent = total > 0 ? (int)Math.Round(processed * 100.0 / total) : 100;
                Report(progress, "Running", percent, $"Linking {processed:N0} / {total:N0} players…", processed);
            }
        }

        if (pendingUpdates.Count > 0)
            await _repository.BulkUpdatePlayerMetadataLinksAsync(pendingUpdates, cancellationToken);

        return aggregate;
    }

    private static Dictionary<string, List<FidePlayer>> BuildFideSurnameIndex(IReadOnlyList<FidePlayer> catalog)
    {
        var index = new Dictionary<string, List<FidePlayer>>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in catalog)
        {
            if (!index.TryGetValue(row.Surname, out var list))
            {
                list = [];
                index[row.Surname] = list;
            }

            list.Add(row);
        }

        return index;
    }

    private LinkOutcome ComputeOutcome(
        Player player,
        short? referenceGameYear,
        IReadOnlyList<FidePlayer> fideCandidates,
        IReadOnlyList<WorldChampion> worldChampions)
    {
        var matchedWorldChampionId = _worldChampionMatcher.Match(player.Surname, player.Forenames, worldChampions);
        var newWorldChampionId = matchedWorldChampionId ?? player.WorldChampionId;

        var matchedFidePlayerId = _fidePlayerMatcher.Match(
            player.Surname,
            player.Forenames,
            fideCandidates,
            referenceGameYear);

        var worldChampionChanged = matchedWorldChampionId.HasValue
            && matchedWorldChampionId != player.WorldChampionId;
        var fideLinked = matchedFidePlayerId.HasValue && matchedFidePlayerId != player.FidePlayerId;
        var fideCleared = player.FidePlayerId.HasValue && !matchedFidePlayerId.HasValue;
        var unchanged = !worldChampionChanged && !fideLinked && !fideCleared;

        return new LinkOutcome(
            worldChampionChanged || fideLinked || fideCleared,
            newWorldChampionId,
            matchedFidePlayerId,
            PlayerMetadataLinkResult.Empty.WithSinglePlayer(
                worldChampionChanged,
                fideLinked,
                fideCleared,
                unchanged));
    }

    private static void Report(
        IProgress<MaintenanceProgress>? progress,
        string status,
        int? percent,
        string? message,
        int rowsProcessed)
    {
        progress?.Report(new MaintenanceProgress
        {
            Operation = "LinkPlayerMetadata",
            Status = status,
            PercentComplete = percent,
            Message = message,
            RowsProcessed = rowsProcessed
        });
    }

    private sealed record LinkOutcome(
        bool ShouldUpdate,
        int? WorldChampionId,
        int? FidePlayerId,
        PlayerMetadataLinkResult Result);
}
