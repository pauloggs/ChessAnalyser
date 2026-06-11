using Interfaces.DTO;
using Repositories;

namespace Services.PlayerMetadata;

/// <inheritdoc />
public sealed class PlayerFideMetadataEnricher(
    IChessRepository repository,
    IFidePlayerMatcher fidePlayerMatcher) : IPlayerFideMetadataEnricher
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IFidePlayerMatcher _fidePlayerMatcher =
        fidePlayerMatcher ?? throw new ArgumentNullException(nameof(fidePlayerMatcher));

    /// <inheritdoc />
    public async Task<bool> TryEnrichPlayerAsync(
        int playerId,
        string surname,
        string forenames,
        CancellationToken cancellationToken = default)
    {
        await _fidePlayerMatcher.EnsureLoadedAsync(cancellationToken).ConfigureAwait(false);

        var activity = await _repository.GetPlayerCorpusActivityAsync(playerId, cancellationToken).ConfigureAwait(false);
        var match = _fidePlayerMatcher.Match(
            surname,
            forenames,
            new FidePlayerMatchContext
            {
                CorpusFirstGameYear = activity?.FirstGameYear,
                CorpusLastGameYear = activity?.LastGameYear
            });

        if (match.Outcome != FidePlayerMatchOutcome.Matched || match.Record is null)
            return false;

        var metadata = ToMetadata(match.Record);
        if (metadata.FideId is int fideId)
        {
            var existingOwner = await _repository.GetPlayerIdByFideIdAsync(fideId, cancellationToken).ConfigureAwait(false);
            if (existingOwner is int ownerId && ownerId != playerId)
                return false;
        }

        await _repository.UpdatePlayerFideMetadataAsync(playerId, metadata, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<FideMetadataSyncResult> BackfillAllAsync(
        bool dryRun = false,
        CancellationToken cancellationToken = default)
    {
        await _fidePlayerMatcher.EnsureLoadedAsync(cancellationToken).ConfigureAwait(false);

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
            var context = new FidePlayerMatchContext
            {
                KnownBirthYear = player.BirthYear,
                CorpusFirstGameYear = activity?.FirstGameYear,
                CorpusLastGameYear = activity?.LastGameYear
            };

            var match = _fidePlayerMatcher.Match(player.Surname, player.Forenames, context);
            switch (match.Outcome)
            {
                case FidePlayerMatchOutcome.Matched:
                    matched++;
                    var metadata = ToMetadata(match.Record!);
                    if (MetadataEquals(player, metadata))
                        break;

                    if (metadata.FideId is int fideId &&
                        fideIdOwner.TryGetValue(fideId, out var ownerId) &&
                        ownerId != player.Id)
                    {
                        fideIdConflict++;
                        break;
                    }

                    if (!dryRun)
                    {
                        await _repository.UpdatePlayerFideMetadataAsync(player.Id, metadata, cancellationToken)
                            .ConfigureAwait(false);
                    }

                    if (metadata.FideId is int assignedFideId)
                        fideIdOwner[assignedFideId] = player.Id;

                    updated++;
                    break;
                case FidePlayerMatchOutcome.Ambiguous:
                    ambiguous++;
                    if (await TryClearIncompatibleMetadataAsync(player, activity, dryRun, cancellationToken))
                        updated++;
                    break;
                default:
                    unmatched++;
                    if (await TryClearIncompatibleMetadataAsync(player, activity, dryRun, cancellationToken))
                        updated++;
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
        PlayerCorpusActivity? activity,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        if (player.FideId is null)
            return false;

        var context = new FidePlayerMatchContext
        {
            KnownBirthYear = player.BirthYear,
            CorpusFirstGameYear = activity?.FirstGameYear,
            CorpusLastGameYear = activity?.LastGameYear
        };

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
}
