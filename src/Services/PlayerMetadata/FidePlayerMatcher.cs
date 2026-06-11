using Interfaces.DTO;
using Repositories;
using Services.Helpers;

namespace Services.PlayerMetadata;

/// <inheritdoc />
public sealed class FidePlayerMatcher : IFidePlayerMatcher
{
    private readonly IChessRepository? _repository;
    private IReadOnlyList<FidePlayerRecord> _records = Array.Empty<FidePlayerRecord>();
    private Dictionary<string, List<FidePlayerRecord>> _bySurname = new(StringComparer.OrdinalIgnoreCase);
    private bool _loaded;

    public FidePlayerMatcher(IChessRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>Parameterless constructor for unit tests that call <see cref="SetRecords"/> directly.</summary>
    public FidePlayerMatcher()
    {
    }

    /// <inheritdoc />
    public async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
    {
        if (_loaded || _repository == null)
            return;

        var records = await _repository.GetFidePlayers(cancellationToken).ConfigureAwait(false);
        SetRecords(records);
        _loaded = true;
    }

    /// <inheritdoc />
    public void InvalidateCache()
    {
        _loaded = false;
        _records = Array.Empty<FidePlayerRecord>();
        _bySurname = new Dictionary<string, List<FidePlayerRecord>>(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public void LoadCatalogSnapshot(IReadOnlyList<FidePlayerRecord> records)
    {
        SetRecords(records);
        _loaded = true;
    }

    /// <summary>Indexes FIDE rows for matching. Used by tests and after loading from <c>Ref.FidePlayer</c>.</summary>
    public void SetRecords(IReadOnlyList<FidePlayerRecord> records)
    {
        _records = records ?? throw new ArgumentNullException(nameof(records));
        _bySurname = records
            .GroupBy(r => r.Surname.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public FidePlayerMatchResult Match(string surname, string? forenames, FidePlayerMatchContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(surname))
            return Unmatched();

        if (!_bySurname.TryGetValue(surname.Trim(), out var candidates))
            return Unmatched();

        var forenamesNorm = (forenames ?? string.Empty).Trim();
        var matches = candidates
            .Where(c => PlayerForenamesMatcher.ForenamesMatch(c.Forenames, forenamesNorm))
            .ToList();

        if (matches.Count == 0)
            return Unmatched();

        if (matches.Count == 1)
            return Matched(matches[0]);

        return ResolveAmbiguous(matches, context);
    }

    private static FidePlayerMatchResult ResolveAmbiguous(
        IReadOnlyList<FidePlayerRecord> matches,
        FidePlayerMatchContext? context)
    {
        var scored = matches
            .Select(m => (Record: m, Score: ScoreCandidate(m, context)))
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Record.FideId)
            .ToList();

        if (scored[0].Score >= 100)
            return Matched(scored[0].Record);

        if (scored.Count >= 2 && scored[0].Score >= 30 && scored[0].Score - scored[1].Score >= 20)
            return Matched(scored[0].Record);

        return new FidePlayerMatchResult { Outcome = FidePlayerMatchOutcome.Ambiguous };
    }

    internal static int ScoreCandidate(FidePlayerRecord candidate, FidePlayerMatchContext? context)
    {
        if (context is null)
            return 0;

        var score = 0;

        if (context.KnownBirthYear is short knownBirth && candidate.BirthYear == knownBirth)
            score += 100;
        else if (context.KnownBirthYear is short kb && candidate.BirthYear is short cb &&
                 Math.Abs(kb - cb) <= 1)
            score += 50;

        if (context.CorpusFirstGameYear is short first &&
            context.CorpusLastGameYear is short last &&
            candidate.BirthYear is short birth)
        {
            if (first >= birth + 8 && last <= birth + 100)
                score += 30;
            else if (first >= birth + 5 && last <= birth + 110)
                score += 10;
        }

        return score;
    }

    private static FidePlayerMatchResult Matched(FidePlayerRecord record) =>
        new() { Outcome = FidePlayerMatchOutcome.Matched, Record = record };

    private static FidePlayerMatchResult Unmatched() =>
        new() { Outcome = FidePlayerMatchOutcome.Unmatched };
}
