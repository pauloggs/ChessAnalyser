using Interfaces.DTO;
using Repositories;
using Services.PlayerMetadata;

namespace Services;

/// <inheritdoc />
public sealed class PlayerMetadataSyncService(
    IChessRepository repository,
    IWorldChampionMatcher worldChampionMatcher,
    IFideRatingListReader fideRatingListReader,
    IFidePlayerMatcher fidePlayerMatcher) : IPlayerMetadataSyncService
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IWorldChampionMatcher _worldChampionMatcher =
        worldChampionMatcher ?? throw new ArgumentNullException(nameof(worldChampionMatcher));
    private readonly IFideRatingListReader _fideRatingListReader =
        fideRatingListReader ?? throw new ArgumentNullException(nameof(fideRatingListReader));
    private readonly IFidePlayerMatcher _fidePlayerMatcher =
        fidePlayerMatcher ?? throw new ArgumentNullException(nameof(fidePlayerMatcher));

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
    public async Task<FideMetadataSyncResult> SyncFideMetadataAsync(
        string fideListPath,
        bool dryRun = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fideListPath);

        var records = await _fideRatingListReader.ReadAsync(fideListPath, cancellationToken).ConfigureAwait(false);
        _fidePlayerMatcher.SetRecords(records);

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
}
