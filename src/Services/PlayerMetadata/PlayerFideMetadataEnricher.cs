using Interfaces.DTO;
using Repositories;

namespace Services.PlayerMetadata;

/// <inheritdoc />
public sealed class PlayerFideMetadataEnricher(
    IChessRepository repository,
    IFidePlayerMatcher fidePlayerMatcher,
    IWorldChampionMatcher worldChampionMatcher) : IPlayerFideMetadataEnricher
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IFidePlayerMatcher _fidePlayerMatcher =
        fidePlayerMatcher ?? throw new ArgumentNullException(nameof(fidePlayerMatcher));
    private readonly IWorldChampionMatcher _worldChampionMatcher =
        worldChampionMatcher ?? throw new ArgumentNullException(nameof(worldChampionMatcher));

    /// <inheritdoc />
    public Task<bool> TryEnrichPlayerAsync(
        int playerId,
        short? observedGameYear = null,
        CancellationToken cancellationToken = default) =>
        TryEnrichPlayerAsync(playerId, surname: null, forenames: null, observedGameYear, cancellationToken);

    /// <inheritdoc />
    public async Task<bool> TryEnrichPlayerAsync(
        int playerId,
        string? surname,
        string? forenames,
        short? observedGameYear = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureCatalogsLoadedAsync(cancellationToken).ConfigureAwait(false);

        var player = await _repository.GetPlayerByIdAsync(playerId, cancellationToken).ConfigureAwait(false);
        if (player is null)
            return false;

        if (surname is not null)
            player.Surname = surname;
        if (forenames is not null)
            player.Forenames = forenames;

        var activity = await _repository.GetPlayerCorpusActivityAsync(playerId, cancellationToken).ConfigureAwait(false);
        var context = FidePlayerMatchContextBuilder.FromActivity(activity, observedGameYear);

        var updated = false;
        if (await SyncWorldChampionFlagAsync(player, dryRun: false, cancellationToken).ConfigureAwait(false))
            updated = true;

        var result = await ApplyFideMetadataAsync(player, context, dryRun: false, fideIdOwner: null, cancellationToken)
            .ConfigureAwait(false);
        return updated || result.Updated;
    }

    /// <inheritdoc />
    public async Task<FideMetadataSyncResult> EnrichAllAsync(
        bool dryRun = false,
        CancellationToken cancellationToken = default)
    {
        await EnsureCatalogsLoadedAsync(cancellationToken).ConfigureAwait(false);

        var players = (await _repository.GetPlayers().ConfigureAwait(false))
            .OrderBy(p => p.Id)
            .ToList();
        var updated = 0;
        var matched = 0;
        var unmatched = 0;
        var ambiguous = 0;
        var fideIdConflict = 0;

        var fideIdOwner = new Dictionary<int, int>();
        foreach (var existing in players)
        {
            if (existing.FideId is int existingFideId)
                fideIdOwner[existingFideId] = existing.Id;
        }

        foreach (var player in players)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var activity = await _repository.GetPlayerCorpusActivityAsync(player.Id, cancellationToken)
                .ConfigureAwait(false);
            var context = FidePlayerMatchContextBuilder.FromActivity(activity);

            if (await SyncWorldChampionFlagAsync(player, dryRun, cancellationToken).ConfigureAwait(false))
                updated++;

            var result = await ApplyFideMetadataAsync(player, context, dryRun, fideIdOwner, cancellationToken)
                .ConfigureAwait(false);

            if (result.Updated)
                updated++;

            switch (result.MatchOutcome)
            {
                case FidePlayerMatchOutcome.Matched:
                    matched++;
                    if (result.FideIdConflict)
                        fideIdConflict++;
                    break;
                case FidePlayerMatchOutcome.Ambiguous:
                    ambiguous++;
                    break;
                default:
                    unmatched++;
                    break;
            }
        }

        return new FideMetadataSyncResult
        {
            PlayersChecked = players.Count,
            PlayersUpdated = updated,
            PlayersMatched = matched,
            PlayersUnmatched = unmatched,
            PlayersAmbiguous = ambiguous,
            PlayersFideIdConflict = fideIdConflict,
            DryRun = dryRun
        };
    }

    private async Task EnsureCatalogsLoadedAsync(CancellationToken cancellationToken)
    {
        await _fidePlayerMatcher.EnsureLoadedAsync(cancellationToken).ConfigureAwait(false);
        await _worldChampionMatcher.EnsureLoadedAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> SyncWorldChampionFlagAsync(
        Player player,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var shouldBeChampion = _worldChampionMatcher.IsWorldChampion(player.Surname, player.Forenames);
        if (player.WasWorldChampion == shouldBeChampion)
            return false;

        if (!dryRun)
        {
            await _repository.UpdatePlayerWasWorldChampionAsync(player.Id, shouldBeChampion, cancellationToken)
                .ConfigureAwait(false);
        }

        return true;
    }

    private async Task<FideApplyResult> ApplyFideMetadataAsync(
        Player player,
        FidePlayerMatchContext context,
        bool dryRun,
        Dictionary<int, int>? fideIdOwner,
        CancellationToken cancellationToken)
    {
        var match = _fidePlayerMatcher.Match(player.Surname, player.Forenames, context);
        switch (match.Outcome)
        {
            case FidePlayerMatchOutcome.Matched:
            {
                var metadata = ToMetadata(match.Record!);
                if (MetadataEquals(player, metadata))
                    return new FideApplyResult(FidePlayerMatchOutcome.Matched, Updated: false, FideIdConflict: false);

                if (metadata.FideId is int fideId)
                {
                    if (fideIdOwner is not null &&
                        fideIdOwner.TryGetValue(fideId, out var ownerId) &&
                        ownerId != player.Id)
                        return new FideApplyResult(FidePlayerMatchOutcome.Matched, Updated: false, FideIdConflict: true);

                    var existingOwner = await _repository.GetPlayerIdByFideIdAsync(fideId, cancellationToken)
                        .ConfigureAwait(false);
                    if (existingOwner is int otherId && otherId != player.Id)
                        return new FideApplyResult(FidePlayerMatchOutcome.Matched, Updated: false, FideIdConflict: true);
                }

                if (!dryRun)
                {
                    await _repository.UpdatePlayerFideMetadataAsync(player.Id, metadata, cancellationToken)
                        .ConfigureAwait(false);
                }

                if (metadata.FideId is int assignedFideId)
                    fideIdOwner?[assignedFideId] = player.Id;

                return new FideApplyResult(FidePlayerMatchOutcome.Matched, Updated: true, FideIdConflict: false);
            }
            default:
            {
                var cleared = await TryClearIncompatibleMetadataAsync(player, context, dryRun, cancellationToken)
                    .ConfigureAwait(false);
                return new FideApplyResult(match.Outcome, Updated: cleared, FideIdConflict: false);
            }
        }
    }

    private static PlayerFideMetadata ToMetadata(FidePlayerRecord record) =>
        new()
        {
            FideId = record.FideId,
            Federation = record.Federation,
            Sex = record.Sex,
            FideTitle = record.Title,
            BirthYear = record.BirthYear
        };

    private static bool MetadataEquals(Player player, PlayerFideMetadata metadata) =>
        player.FideId == metadata.FideId &&
        string.Equals(player.Federation, metadata.Federation, StringComparison.Ordinal) &&
        string.Equals(player.Sex, metadata.Sex, StringComparison.Ordinal) &&
        string.Equals(player.FideTitle, metadata.FideTitle, StringComparison.Ordinal) &&
        player.BirthYear == metadata.BirthYear;

    private async Task<bool> TryClearIncompatibleMetadataAsync(
        Player player,
        FidePlayerMatchContext context,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        if (player.FideId is null)
            return false;

        if (FidePlayerMatcher.IsCorpusCompatible(player.BirthYear, context))
            return false;

        if (!dryRun)
        {
            await _repository.UpdatePlayerFideMetadataAsync(
                player.Id,
                new PlayerFideMetadata(),
                cancellationToken).ConfigureAwait(false);
        }

        return true;
    }

    private readonly record struct FideApplyResult(
        FidePlayerMatchOutcome MatchOutcome,
        bool Updated,
        bool FideIdConflict);
}
