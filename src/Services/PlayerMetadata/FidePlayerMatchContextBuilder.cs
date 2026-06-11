using Interfaces.DTO;

namespace Services.PlayerMetadata;

/// <summary>Builds <see cref="FidePlayerMatchContext"/> from persisted games and an optional in-flight game year.</summary>
internal static class FidePlayerMatchContextBuilder
{
    internal static FidePlayerMatchContext FromActivity(
        PlayerCorpusActivity? activity,
        short? observedGameYear = null)
    {
        short? first = activity?.FirstGameYear;
        short? last = activity?.LastGameYear;

        if (observedGameYear is short year)
        {
            first = first is null ? year : Math.Min(first.Value, year);
            last = last is null ? year : Math.Max(last.Value, year);
        }

        return new FidePlayerMatchContext
        {
            CorpusFirstGameYear = first,
            CorpusLastGameYear = last
        };
    }
}
