using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Services.PlayerMetadata;

namespace Migrations.Seeding;

/// <summary>
/// Idempotent player metadata enrichment after migrations (Ref catalogs + corpus game years).
/// </summary>
internal static class PlayerMetadataBackfillRunner
{
    internal static async Task RunAsync(IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddScoped<IChessRepository, ChessRepository>();
        services.AddScoped<IWorldChampionMatcher, WorldChampionMatcher>();
        services.AddScoped<IFidePlayerMatcher, FidePlayerMatcher>();
        services.AddScoped<IPlayerFideMetadataEnricher, PlayerFideMetadataEnricher>();

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var enricher = scope.ServiceProvider.GetRequiredService<IPlayerFideMetadataEnricher>();

        var result = await enricher.EnrichAllAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        Console.WriteLine(
            "Player metadata enrichment: checked={0}, matched={1}, updated={2}, unmatched={3}, ambiguous={4}, fideIdConflict={5}.",
            result.PlayersChecked,
            result.PlayersMatched,
            result.PlayersUpdated,
            result.PlayersUnmatched,
            result.PlayersAmbiguous,
            result.PlayersFideIdConflict);
    }
}
