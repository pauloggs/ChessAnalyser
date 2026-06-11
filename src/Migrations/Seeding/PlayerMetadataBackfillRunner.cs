using Interfaces.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Services;
using Services.PlayerMetadata;

namespace Migrations.Seeding;

/// <summary>
/// Applies Ref-catalog metadata onto existing <see cref="Player"/> rows after the FIDE catalog is seeded.
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
        services.AddScoped<IPlayerMetadataSyncService, PlayerMetadataSyncService>();

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var sync = scope.ServiceProvider.GetRequiredService<IPlayerMetadataSyncService>();

        var wc = await sync.SyncWorldChampionFlagsAsync(cancellationToken).ConfigureAwait(false);
        Console.WriteLine(
            "Player world-champion backfill: checked={0}, updated={1}.",
            wc.PlayersChecked,
            wc.PlayersUpdated);

        var fide = await sync.BackfillFideMetadataAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        Console.WriteLine(
            "Player FIDE metadata backfill: checked={0}, matched={1}, updated={2}, unmatched={3}, ambiguous={4}, fideIdConflict={5}.",
            fide.PlayersChecked,
            fide.PlayersMatched,
            fide.PlayersUpdated,
            fide.PlayersUnmatched,
            fide.PlayersAmbiguous,
            fide.PlayersFideIdConflict);
    }
}
