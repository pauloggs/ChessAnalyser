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

    public Task<PagedResult<Game>> GetGamesPage(int page, int pageSize, GamePageFilters? filters, CancellationToken cancellationToken = default) =>
        Throw<PagedResult<Game>>();

    public Task<List<string>> GetProcessedGameIds() => Throw<List<string>>();

    public Task<List<Player>> GetPlayers() => Throw<List<Player>>();

    public Task<Player?> GetPlayerByIdAsync(int playerId, CancellationToken cancellationToken = default) =>
        Throw<Player?>();

    public Task<int?> GetPlayerIdBySurnameAndForenames(string surname, string forenames) => Throw<int?>();

    public Task<List<Player>> GetPlayersBySurname(string surname) => Throw<List<Player>>();

    public Task<int> InsertPlayer(Player player) => Throw<int>();

    public Task UpdatePlayerWasWorldChampionAsync(int playerId, bool wasWorldChampion, CancellationToken cancellationToken = default) =>
        Throw();

    public Task UpdatePlayerFideMetadataAsync(int playerId, PlayerFideMetadata metadata, CancellationToken cancellationToken = default) =>
        Throw();

    public Task<IReadOnlyList<WorldChampionRef>> GetWorldChampions(CancellationToken cancellationToken = default) =>
        Throw<IReadOnlyList<WorldChampionRef>>();

    public Task<IReadOnlyList<FidePlayerRecord>> GetFidePlayers(CancellationToken cancellationToken = default) =>
        Throw<IReadOnlyList<FidePlayerRecord>>();

    public Task ReplaceFideCatalogAsync(IReadOnlyList<FidePlayerRecord> records, CancellationToken cancellationToken = default) =>
        Throw();

    public Task<int?> GetPlayerIdByFideIdAsync(int fideId, CancellationToken cancellationToken = default) =>
        Throw<int?>();

    public Task<PlayerCorpusActivity?> GetPlayerCorpusActivityAsync(int playerId, CancellationToken cancellationToken = default) =>
        Throw<PlayerCorpusActivity?>();

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

    public Task<IReadOnlyList<GameCountByEcoRow>> GetGameCountsByEcoAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<GameCountByEcoRow>>();

    public Task<IReadOnlyList<GameCountByYearRow>> GetGameCountsByYearAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<GameCountByYearRow>>();

    public Task<IReadOnlyList<GameCountByResultRow>> GetGameCountsByResultAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<GameCountByResultRow>>();

    public Task<IReadOnlyList<GameCountByPlayerRow>> GetGameCountsByPlayerAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<GameCountByPlayerRow>>();

    public Task<IReadOnlyList<PlayerResultSummaryRow>> GetPlayerResultSummariesAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerResultSummaryRow>>();

    public Task<IReadOnlyList<PlayerMaterialAverageRow>> GetPlayerMaterialAveragesAtPlyAsync(
        AnalyticsQuery query,
        int moveNumber,
        int plyIndex,
        string colourMode,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerMaterialAverageRow>>();

    public Task<IReadOnlyList<AverageCastlingPlyRow>> GetAverageCastlingPlyAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<AverageCastlingPlyRow>>();

    public Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerAverageCastlingPlyAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerStylePerPlayerMetricRow>>();

    public Task<IReadOnlyList<AverageMaterialVolatilityRow>> GetAverageMaterialVolatilityAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<AverageMaterialVolatilityRow>>();

    public Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerAverageMaterialVolatilityAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerStylePerPlayerMetricRow>>();

    public Task<IReadOnlyList<BishopPairFrequencyRow>> GetBishopPairFrequencyAsync(
        AnalyticsQuery query,
        int? minPlyIndex,
        int? maxPlyIndex,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<BishopPairFrequencyRow>>();

    public Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerBishopPairFrequencyAsync(
        AnalyticsQuery query,
        int? minPlyIndex,
        int? maxPlyIndex,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerStylePerPlayerMetricRow>>();

    public Task<IReadOnlyList<MinorPieceCompositionRow>> GetMinorPieceCompositionAsync(
        AnalyticsQuery query,
        int? minPlyIndex,
        int? maxPlyIndex,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<MinorPieceCompositionRow>>();

    public Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerMinorPieceCompositionAsync(
        AnalyticsQuery query,
        int? minPlyIndex,
        int? maxPlyIndex,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerStylePerPlayerMetricRow>>();

    public Task<IReadOnlyList<CaptureRateRow>> GetCaptureRateAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<CaptureRateRow>>();

    public Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerCaptureRateAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerStylePerPlayerMetricRow>>();

    public Task<IReadOnlyList<QueenTradeRateRow>> GetQueenTradeRateAsync(
        AnalyticsQuery query,
        int queenTradeMaxPly,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<QueenTradeRateRow>>();

    public Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerQueenTradeRateAsync(
        AnalyticsQuery query,
        int queenTradeMaxPly,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerStylePerPlayerMetricRow>>();

    public Task<IReadOnlyList<CentreMoveRateRow>> GetCentreMoveRateAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<CentreMoveRateRow>>();

    public Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerCentreMoveRateAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerStylePerPlayerMetricRow>>();

    public Task<IReadOnlyList<ForwardMoveRateRow>> GetForwardMoveRateAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<ForwardMoveRateRow>>();

    public Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerForwardMoveRateAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerStylePerPlayerMetricRow>>();

    public Task<IReadOnlyList<CastlingSidePreferenceRow>> GetCastlingSidePreferenceAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<CastlingSidePreferenceRow>>();

    public Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerCastlingSidePreferenceAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerStylePerPlayerMetricRow>>();

    public Task<IReadOnlyList<OppositeSideCastlingRateRow>> GetOppositeSideCastlingRateAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<OppositeSideCastlingRateRow>>();

    public Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerOppositeSideCastlingRateAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerStylePerPlayerMetricRow>>();

    public Task<IReadOnlyList<UncastledKingRateRow>> GetUncastledKingRateAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<UncastledKingRateRow>>();

    public Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerUncastledKingRateAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerStylePerPlayerMetricRow>>();

    public Task<IReadOnlyList<FirstQueenMovePlyRow>> GetFirstQueenMovePlyAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<FirstQueenMovePlyRow>>();

    public Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerFirstQueenMovePlyAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerStylePerPlayerMetricRow>>();

    public Task<IReadOnlyList<AverageGameLengthRow>> GetAverageGameLengthAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<AverageGameLengthRow>>();

    public Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerAverageGameLengthAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerStylePerPlayerMetricRow>>();

    public Task<IReadOnlyList<ShortDrawRateRow>> GetShortDrawRateAsync(
        AnalyticsQuery query,
        int shortDrawMaxPly,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<ShortDrawRateRow>>();

    public Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerShortDrawRateAsync(
        AnalyticsQuery query,
        int shortDrawMaxPly,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerStylePerPlayerMetricRow>>();

    public Task<IReadOnlyList<EcoDiversityRow>> GetEcoDiversityAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<EcoDiversityRow>>();

    public Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerEcoDiversityAsync(
        AnalyticsQuery query,
        CancellationToken cancellationToken = default) => Throw<IReadOnlyList<PlayerStylePerPlayerMetricRow>>();

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
