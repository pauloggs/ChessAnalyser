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

        var matchedWorldChampionId = _worldChampionMatcher.Match(player.Surname, player.Forenames, _worldChampions);
        var newWorldChampionId = matchedWorldChampionId ?? player.WorldChampionId;

        var fideCandidates = await _repository.GetFidePlayersBySurnameAsync(player.Surname, cancellationToken);
        var matchedFidePlayerId = _fidePlayerMatcher.Match(
            player.Surname,
            player.Forenames,
            fideCandidates,
            referenceGameYear);

        var newFidePlayerId = await ResolveFidePlayerIdAsync(
            player,
            matchedFidePlayerId,
            fideCandidates,
            referenceGameYear,
            cancellationToken);

        var worldChampionChanged = matchedWorldChampionId.HasValue
            && matchedWorldChampionId != player.WorldChampionId;
        var fideLinked = newFidePlayerId.HasValue && newFidePlayerId != player.FidePlayerId;
        var fideCleared = player.FidePlayerId.HasValue && !newFidePlayerId.HasValue;
        var unchanged = !worldChampionChanged && !fideLinked && !fideCleared;

        if (worldChampionChanged || fideLinked || fideCleared)
        {
            await _repository.UpdatePlayerMetadataLinksAsync(
                playerId,
                newWorldChampionId,
                newFidePlayerId,
                cancellationToken);
        }

        return PlayerMetadataLinkResult.Empty.WithSinglePlayer(
            worldChampionChanged,
            fideLinked,
            fideCleared,
            unchanged);
    }

    /// <inheritdoc />
    public async Task<PlayerMetadataLinkResult> LinkAllPlayersAsync(CancellationToken cancellationToken = default)
    {
        _worldChampions = await _repository.GetWorldChampionsAsync(cancellationToken);
        var players = await _repository.GetPlayers();
        var aggregate = PlayerMetadataLinkResult.Empty;

        foreach (var player in players)
        {
            var outcome = await TryLinkPlayerAsync(player.Id, gameYear: null, cancellationToken);
            aggregate = aggregate.Add(outcome);
        }

        return aggregate;
    }

    private async Task<int?> ResolveFidePlayerIdAsync(
        Player player,
        int? matchedFidePlayerId,
        IReadOnlyList<FidePlayer> surnameCandidates,
        short? referenceGameYear,
        CancellationToken cancellationToken)
    {
        if (matchedFidePlayerId.HasValue)
            return matchedFidePlayerId;

        if (!player.FidePlayerId.HasValue)
            return null;

        var linked = surnameCandidates.FirstOrDefault(c => c.Id == player.FidePlayerId.Value)
            ?? await _repository.GetFidePlayerByIdAsync(player.FidePlayerId.Value, cancellationToken);

        if (linked is null)
            return player.FidePlayerId;

        return FidePlayerMatcher.IsBirthYearCompatible(linked.BirthYear, referenceGameYear)
            ? player.FidePlayerId
            : null;
    }
}
