using Interfaces.Analytics;
using Interfaces.DTO;
using Microsoft.Extensions.Configuration;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// <see cref="IChessRepository"/> used only for in-process materialization perf smoke: analytics replace methods no-op;
/// all other members throw if called.
/// </summary>
public sealed class MaterializationWriteOnlyNoOpRepository : IChessRepository
{
    public IConfiguration Configuration =>
        throw new InvalidOperationException($"{nameof(MaterializationWriteOnlyNoOpRepository)} does not expose configuration.");

    public Task<List<Game>> GetGames() => Throw<List<Game>>();

    public Task<List<string>> GetProcessedGameIds() => Throw<List<string>>();

    public Task<List<Player>> GetPlayers() => Throw<List<Player>>();

    public Task<int?> GetPlayerIdBySurnameAndForenames(string surname, string forenames) => Throw<int?>();

    public Task<List<Player>> GetPlayersBySurname(string surname) => Throw<List<Player>>();

    public Task<int> InsertPlayer(Player player) => Throw<int>();

    public Task<int> InsertGame(Game game) => Throw<int>();

    public Task InsertBoardPositions(Game game, int gameId) => Throw();

    public Task InsertGameParseError(GameParseError error) => Throw();

    public Task ReplaceGameMovesForGame(int gameId, IReadOnlyList<GameMoveFact> rows) => Task.CompletedTask;

    public Task ReplaceGamePositionSummariesForGame(int gameId, IReadOnlyList<GamePositionSummary> rows) => Task.CompletedTask;

    public Task<List<GameMoveFact>> GetGameMovesForGame(int gameId) => Throw<List<GameMoveFact>>();

    public Task<List<GamePositionSummary>> GetGamePositionSummariesForGame(int gameId) => Throw<List<GamePositionSummary>>();

    public Task<IReadOnlyList<MaterialAverageByYearRow>> GetMaterialAveragesByYearAtPlyAsync(
        AnalyticsQuery query,
        int plyIndex,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<MaterialAverageByYearRow>>();

    public Task<IReadOnlyList<KnightDestinationCountRow>> GetKnightDestinationCountsAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<KnightDestinationCountRow>>();

    public Task<IReadOnlyList<int>> GetGameIdsNeedingAnalyticsBackfillAsync(CancellationToken cancellationToken = default) =>
        Throw<IReadOnlyList<int>>();

    public Task<IReadOnlyList<(int PlyIndex, BoardPosition Position)>> GetBoardPositionsForGameOrderedAsync(
        int gameId,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<(int PlyIndex, BoardPosition Position)>>();

    private static Task<T> Throw<T>() =>
        Task.FromException<T>(new InvalidOperationException("Not supported on write-only no-op repository."));

    private static Task Throw() =>
        Task.FromException(new InvalidOperationException("Not supported on write-only no-op repository."));
}
