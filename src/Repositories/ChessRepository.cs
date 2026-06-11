using Dapper;
using Interfaces.Analytics;
using Interfaces.DTO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositories
{
    public interface IChessRepository
    {
        IConfiguration Configuration { get; }

        /// <summary>
        /// One page of rows from <c>dbo.Game</c> ordered by <c>Id</c>, plus total row count.
        /// Optional <paramref name="filters"/> apply with AND semantics (see <see cref="GamePageFilters"/>).
        /// Honors <paramref name="cancellationToken"/> and uses a bounded command timeout.
        /// </summary>
        Task<PagedResult<Game>> GetGamesPage(
            int page,
            int pageSize,
            GamePageFilters? filters,
            CancellationToken cancellationToken = default);

        Task<List<string>> GetProcessedGameIds();

        Task<List<Player>> GetPlayers();

        /// <summary>Returns the Player Id if a row exists with the given Surname and Forenames; otherwise null.</summary>
        Task<int?> GetPlayerIdBySurnameAndForenames(string surname, string forenames);

        /// <summary>Returns all players with the given Surname (for fuzzy forenames matching).</summary>
        Task<List<Player>> GetPlayersBySurname(string surname);

        Task<int> InsertPlayer(Player player);

        Task<int> InsertGame(Game game);

        /// <summary>
        /// Persists all board positions for a game (initial position and per-ply positions) to dbo.BoardPosition.
        /// Replaces any existing rows for this game.
        /// </summary>
        Task InsertBoardPositions(Game game, int gameId);

        /// <summary>
        /// Inserts a parse error record for diagnostics. Does not reference Game (failed games are not inserted).
        /// </summary>
        Task InsertGameParseError(GameParseError error);

        /// <summary>
        /// Replaces all <c>dbo.GameMove</c> rows for a game (delete then insert). Pass an empty list to clear only.
        /// </summary>
        Task ReplaceGameMovesForGame(int gameId, IReadOnlyList<GameMoveFact> rows);

        /// <summary>
        /// Replaces all <c>dbo.GamePositionSummary</c> rows for a game (delete then insert).
        /// </summary>
        Task ReplaceGamePositionSummariesForGame(int gameId, IReadOnlyList<GamePositionSummary> rows);

        Task<List<GameMoveFact>> GetGameMovesForGame(int gameId);

        Task<List<GamePositionSummary>> GetGamePositionSummariesForGame(int gameId);

        /// <summary>
        /// Average white/black material from <c>GamePositionSummary</c> at <paramref name="plyIndex"/>,
        /// grouped by <c>Game.GameYear</c> (games with null year excluded).
        /// </summary>
        Task<IReadOnlyList<MaterialAverageByYearRow>> GetMaterialAveragesByYearAtPlyAsync(
            AnalyticsQuery query,
            int plyIndex,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts knight moves by <c>ToSquare</c> with optional game filters.
        /// </summary>
        Task<IReadOnlyList<KnightDestinationCountRow>> GetKnightDestinationCountsAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts games by ECO code with optional game filters.
        /// </summary>
        Task<IReadOnlyList<GameCountByEcoRow>> GetGameCountsByEcoAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts games by <c>GameYear</c> with optional game filters.
        /// </summary>
        Task<IReadOnlyList<GameCountByYearRow>> GetGameCountsByYearAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts games by result with optional game filters.
        /// </summary>
        Task<IReadOnlyList<GameCountByResultRow>> GetGameCountsByResultAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts player appearances with optional game filters.
        /// </summary>
        Task<IReadOnlyList<GameCountByPlayerRow>> GetGameCountsByPlayerAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Summarizes results from each player's perspective with optional game filters.
        /// </summary>
        Task<IReadOnlyList<PlayerResultSummaryRow>> GetPlayerResultSummariesAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Average material for Player A compared with Player B or all players at one ply.
        /// </summary>
        Task<IReadOnlyList<PlayerMaterialAverageRow>> GetPlayerMaterialAveragesAtPlyAsync(
            AnalyticsQuery query,
            int moveNumber,
            int plyIndex,
            string colourMode,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mean first-castling ply for a filtered player (games without castling excluded from the average).
        /// </summary>
        Task<IReadOnlyList<AverageCastlingPlyRow>> GetAverageCastlingPlyAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Per-player mean first-castling ply aggregates for corpus benchmarks.
        /// </summary>
        Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerAverageCastlingPlyAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mean per-game standard deviation of signed material balance for a filtered player.
        /// </summary>
        Task<IReadOnlyList<AverageMaterialVolatilityRow>> GetAverageMaterialVolatilityAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Per-player material volatility aggregates for corpus benchmarks.
        /// </summary>
        Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerAverageMaterialVolatilityAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mean per-game share of plies where the filtered player retains both bishops in a ply window.
        /// </summary>
        Task<IReadOnlyList<BishopPairFrequencyRow>> GetBishopPairFrequencyAsync(
            AnalyticsQuery query,
            int? minPlyIndex,
            int? maxPlyIndex,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Per-player bishop-pair frequency aggregates for corpus benchmarks.
        /// </summary>
        Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerBishopPairFrequencyAsync(
            AnalyticsQuery query,
            int? minPlyIndex,
            int? maxPlyIndex,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mean per-game average bishops-minus-knights for the filtered player in a ply window.
        /// </summary>
        Task<IReadOnlyList<MinorPieceCompositionRow>> GetMinorPieceCompositionAsync(
            AnalyticsQuery query,
            int? minPlyIndex,
            int? maxPlyIndex,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Per-player minor-piece composition aggregates for corpus benchmarks.
        /// </summary>
        Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerMinorPieceCompositionAsync(
            AnalyticsQuery query,
            int? minPlyIndex,
            int? maxPlyIndex,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mean per-game capture rate for the filtered player's moves.
        /// </summary>
        Task<IReadOnlyList<CaptureRateRow>> GetCaptureRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Per-player mean capture rate aggregates for corpus benchmarks.
        /// </summary>
        Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerCaptureRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Proportion of games with an early queen trade on or before the ply threshold.
        /// </summary>
        Task<IReadOnlyList<QueenTradeRateRow>> GetQueenTradeRateAsync(
            AnalyticsQuery query,
            int queenTradeMaxPly,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Per-player early queen trade rate aggregates for corpus benchmarks.
        /// </summary>
        Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerQueenTradeRateAsync(
            AnalyticsQuery query,
            int queenTradeMaxPly,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mean per-game share of moves landing on central squares d4, d5, e4, e5.
        /// </summary>
        Task<IReadOnlyList<CentreMoveRateRow>> GetCentreMoveRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Per-player mean centre-move rate aggregates for corpus benchmarks.
        /// </summary>
        Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerCentreMoveRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mean per-game share of moves landing in the opponent's half.
        /// </summary>
        Task<IReadOnlyList<ForwardMoveRateRow>> GetForwardMoveRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Per-player mean forward-move rate aggregates for corpus benchmarks.
        /// </summary>
        Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerForwardMoveRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Kingside vs queenside rates on the filtered player's first castle per game.
        /// </summary>
        Task<IReadOnlyList<CastlingSidePreferenceRow>> GetCastlingSidePreferenceAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Per-player kingside castling preference aggregates for corpus benchmarks.
        /// </summary>
        Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerCastlingSidePreferenceAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Proportion of filtered games where both sides castled to opposite wings.
        /// </summary>
        Task<IReadOnlyList<OppositeSideCastlingRateRow>> GetOppositeSideCastlingRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Per-player opposite-side castling rate aggregates for corpus benchmarks.
        /// </summary>
        Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerOppositeSideCastlingRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Proportion of filtered games where the player never castled.
        /// </summary>
        Task<IReadOnlyList<UncastledKingRateRow>> GetUncastledKingRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Per-player uncastled-king rate aggregates for corpus benchmarks.
        /// </summary>
        Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerUncastledKingRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mean ply of the filtered player's first queen move.
        /// </summary>
        Task<IReadOnlyList<FirstQueenMovePlyRow>> GetFirstQueenMovePlyAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Per-player mean first queen move ply aggregates for corpus benchmarks.
        /// </summary>
        Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerFirstQueenMovePlyAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mean game length in plies for games involving the filtered player.
        /// </summary>
        Task<IReadOnlyList<AverageGameLengthRow>> GetAverageGameLengthAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Per-player mean game length aggregates for corpus benchmarks.
        /// </summary>
        Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerAverageGameLengthAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Game primary keys that have board rows but no <c>GameMove</c> rows (candidates for analytics backfill).
        /// </summary>
        Task<IReadOnlyList<int>> GetGameIdsNeedingAnalyticsBackfillAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads all <c>dbo.BoardPosition</c> rows for a game ordered by <c>PlyIndex</c>.
        /// </summary>
        Task<IReadOnlyList<(int PlyIndex, BoardPosition Position)>> GetBoardPositionsForGameOrderedAsync(
            int gameId,
            CancellationToken cancellationToken = default);
    }

    public class ChessRepository : IChessRepository
    {
        private readonly IConfiguration _config;

        public ChessRepository(IConfiguration config)
        {
            _config = config;
        }

        public IConfiguration Configuration => throw new NotImplementedException();

        /// <inheritdoc />
        public async Task<PagedResult<Game>> GetGamesPage(
            int page,
            int pageSize,
            GamePageFilters? filters,
            CancellationToken cancellationToken = default)
        {
            var connectionString = _config.GetConnectionString("ChessConnection")
                ?? throw new InvalidOperationException("Connection string 'ChessConnection' is not configured.");

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var offset = (page - 1) * pageSize;
            var eco = NormalizeNonEmpty(filters?.Eco);
            var filterParams = new
            {
                MinGameYear = filters?.MinGameYear,
                MaxGameYear = filters?.MaxGameYear,
                WhitePlayerSurname = filters?.WhitePlayerSurname,
                WhitePlayerForenames = filters?.WhitePlayerForenames,
                BlackPlayerSurname = filters?.BlackPlayerSurname,
                BlackPlayerForenames = filters?.BlackPlayerForenames,
                Eco = eco,
                Offset = offset,
                PageSize = pageSize
            };

            const string whereClause =
                """
                LEFT JOIN dbo.Player wp ON wp.Id = g.WhitePlayerId
                LEFT JOIN dbo.Player bp ON bp.Id = g.BlackPlayerId
                WHERE (@MinGameYear IS NULL OR g.[GameYear] >= @MinGameYear)
                  AND (@MaxGameYear IS NULL OR g.[GameYear] <= @MaxGameYear)
                  AND (@WhitePlayerSurname IS NULL OR (wp.Surname = @WhitePlayerSurname AND (@WhitePlayerForenames IS NULL OR wp.Forenames = @WhitePlayerForenames)))
                  AND (@BlackPlayerSurname IS NULL OR (bp.Surname = @BlackPlayerSurname AND (@BlackPlayerForenames IS NULL OR bp.Forenames = @BlackPlayerForenames)))
                  AND (@Eco IS NULL OR g.[Eco] = @Eco)
                """;

            var countSql = "SELECT COUNT(1) FROM dbo.[Game] g " + whereClause;
            var countCmd = new CommandDefinition(
                countSql,
                filterParams,
                commandTimeout: 120,
                cancellationToken: cancellationToken);
            var totalCount = await connection.ExecuteScalarAsync<int>(countCmd).ConfigureAwait(false);

            var pageSql =
                """
                SELECT g.* FROM dbo.[Game] g
                """ + whereClause + """
                ORDER BY g.[Id]
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                """;
            var pageCmd = new CommandDefinition(
                pageSql,
                filterParams,
                commandTimeout: 120,
                cancellationToken: cancellationToken);
            var rows = (await connection.QueryAsync<Game>(pageCmd).ConfigureAwait(false)).ToList();

            return new PagedResult<Game>
            {
                Items = rows,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        private static string? NormalizeNonEmpty(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            var t = value.Trim();
            return t.Length == 0 ? null : t;
        }

        private static string? NormalizeNamePart(string? value)
        {
            if (value == null)
                return null;
            return value.Trim();
        }

        private static string NormalizePlayerColourFilter(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)
                || string.Equals(value.Trim(), "Any", StringComparison.OrdinalIgnoreCase))
                return "Any";

            if (string.Equals(value.Trim(), "White", StringComparison.OrdinalIgnoreCase))
                return "White";

            if (string.Equals(value.Trim(), "Black", StringComparison.OrdinalIgnoreCase))
                return "Black";

            throw new ArgumentException("playerColour must be Any, White, or Black.", "playerColour");
        }

        /// <summary>
        /// Gets an open SQL connection.
        /// </summary>
        public SqlConnection GetOpenConnection()
        {
            var cs = new SqlConnection(_config.GetConnectionString("ChessConnection"));
            cs.Open();
            return cs;
        }

        public async Task<List<Player>> GetPlayers()
        {
            using var connection = GetOpenConnection();
            using (connection)
            {
                var list = (await connection.QueryAsync<Player>(SqlStatements.GetPlayers)).ToList();
                return list;
            }
        }

        public async Task<int?> GetPlayerIdBySurnameAndForenames(string surname, string forenames)
        {
            using var connection = GetOpenConnection();
            using (connection)
            {
                var id = await connection.ExecuteScalarAsync<int?>(SqlStatements.GetPlayerIdBySurnameAndForenames, new { Surname = surname, Forenames = forenames ?? "" });
                return id;
            }
        }

        public async Task<List<Player>> GetPlayersBySurname(string surname)
        {
            using var connection = GetOpenConnection();
            using (connection)
            {
                var list = (await connection.QueryAsync<Player>(SqlStatements.GetPlayersBySurname, new { Surname = surname })).ToList();
                return list;
            }
        }

        public async Task<int> InsertPlayer(Player player)
        {
            using var connection = GetOpenConnection();
            using (connection)
            {
                var id = await connection.ExecuteScalarAsync<int>(SqlStatements.InsertPlayer, new { player.Surname, player.Forenames });
                return id;
            }
        }

        public async Task<List<string>> GetProcessedGameIds()
        {
            try
            {
                List<string> gameIds = [];

                using var connection = GetOpenConnection();

                using (connection)
                {
                    var sql = SqlStatements.GetGameIds;

                    SqlCommand command = new SqlCommand(sql, connection);

                    using (SqlDataReader reader = command.ExecuteReader())
            {
                        while (reader.Read())
                        {
                            gameIds.Add(reader.GetString(0)); // Assuming the column is of type string
                        }
                    }

                    return gameIds ?? [];
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Inserts a game into the database and returns the newly created Game Id.
        /// </summary>
        /// <param name="game"></param>
        /// <exception cref="Exception"></exception>
        public async Task<int> InsertGame(Game game)
        {
            try
            {
                using var connection = GetOpenConnection();

                using (connection)
                {
                    var sql = SqlStatements.InsertGame;

                    var parameters = new
                    {
                        game.Name,
                        game.GameId,
                        Winner = game.Winner ?? "None",
                        game.WhitePlayerId,
                        game.BlackPlayerId,
                        game.Event,
                        game.Site,
                        game.DateTag,
                        game.GameYear,
                        game.Eco
                    };

                    var gameId = await connection.ExecuteScalarAsync<int>(sql, parameters);

                    return gameId;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Persists all board positions for a game to dbo.BoardPosition. Replaces any existing rows for this game.
        /// </summary>
        public async Task InsertBoardPositions(Game game, int gameId)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            using var connection = GetOpenConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(
                    SqlStatements.DeleteBoardPositionsForGame,
                    new { GameId = gameId },
                    transaction);

                var positionsToInsert = new List<(int PlyIndex, BoardPosition Position)>();

                if (game.BoardPositions != null)
                {
                    foreach (var kvp in game.BoardPositions.OrderBy(k => k.Key))
                        positionsToInsert.Add((kvp.Key, kvp.Value));
                }
                // Persist initial position if set and not already in BoardPositions (e.g. -1)
                if (game.InitialBoardPosition != null && (game.BoardPositions == null || !game.BoardPositions.ContainsKey(-1)))
                    positionsToInsert.Add((-1, game.InitialBoardPosition));

                foreach (var (plyIndex, position) in positionsToInsert.OrderBy(x => x.PlyIndex))
                {
                    var row = new
                    {
                        GameId = gameId,
                        PlyIndex = plyIndex,
                        WP = (long)position.PiecePositions["WP"],
                        WN = (long)position.PiecePositions["WN"],
                        WB = (long)position.PiecePositions["WB"],
                        WR = (long)position.PiecePositions["WR"],
                        WQ = (long)position.PiecePositions["WQ"],
                        WK = (long)position.PiecePositions["WK"],
                        BP = (long)position.PiecePositions["BP"],
                        BN = (long)position.PiecePositions["BN"],
                        BB = (long)position.PiecePositions["BB"],
                        BR = (long)position.PiecePositions["BR"],
                        BQ = (long)position.PiecePositions["BQ"],
                        BK = (long)position.PiecePositions["BK"],
                        EnPassantTargetFile = position.EnPassantTargetFile.HasValue
                            ? position.EnPassantTargetFile.Value.ToString()
                            : (string?)null
                    };

                    await connection.ExecuteAsync(
                        SqlStatements.InsertBoardPosition,
                        row,
                        transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Inserts a single parse error into dbo.GameParseError.
        /// </summary>
        public async Task InsertGameParseError(GameParseError error)
        {
            using var connection = GetOpenConnection();
            await connection.ExecuteAsync(
                SqlStatements.InsertGameParseError,
                new
                {
                    error.SourcePgnFileName,
                    error.GameIndexInFile,
                    error.GameName,
                    error.ErrorMessage
                });
        }

        /// <inheritdoc />
        public async Task ReplaceGameMovesForGame(int gameId, IReadOnlyList<GameMoveFact> rows)
        {
            rows ??= Array.Empty<GameMoveFact>();

            using var connection = GetOpenConnection();
            using var transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync(
                    SqlStatements.DeleteGameMovesForGame,
                    new { GameId = gameId },
                    transaction);

                foreach (var r in rows.OrderBy(x => x.PlyIndex))
                {
                    await connection.ExecuteAsync(
                        SqlStatements.InsertGameMove,
                        new
                        {
                            GameId = gameId,
                            r.PlyIndex,
                            MovingSide = r.MovingSide.ToString(),
                            r.FromSquare,
                            r.ToSquare,
                            MovedPiece = r.MovedPiece.ToString(),
                            CapturedPiece = r.CapturedPiece?.ToString(),
                            PromotionPiece = r.PromotionPiece?.ToString(),
                            r.IsCastlingKingside,
                            r.IsCastlingQueenside
                        },
                        transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <inheritdoc />
        public async Task ReplaceGamePositionSummariesForGame(int gameId, IReadOnlyList<GamePositionSummary> rows)
        {
            rows ??= Array.Empty<GamePositionSummary>();

            using var connection = GetOpenConnection();
            using var transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync(
                    SqlStatements.DeleteGamePositionSummariesForGame,
                    new { GameId = gameId },
                    transaction);

                foreach (var r in rows.OrderBy(x => x.PlyIndex))
                {
                    await connection.ExecuteAsync(
                        SqlStatements.InsertGamePositionSummary,
                        new
                        {
                            GameId = gameId,
                            r.PlyIndex,
                            r.WhiteMaterial,
                            r.BlackMaterial,
                            r.WhitePawnCount,
                            r.WhiteKnightCount,
                            r.WhiteBishopCount,
                            r.WhiteRookCount,
                            r.WhiteQueenCount,
                            r.WhiteKingCount,
                            r.BlackPawnCount,
                            r.BlackKnightCount,
                            r.BlackBishopCount,
                            r.BlackRookCount,
                            r.BlackQueenCount,
                            r.BlackKingCount
                        },
                        transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<List<GameMoveFact>> GetGameMovesForGame(int gameId)
        {
            using var connection = GetOpenConnection();
            var raw = (await connection.QueryAsync<GameMoveRow>(
                SqlStatements.GetGameMovesForGame,
                new { GameId = gameId })).ToList();

            return raw.Select(Map).ToList();
        }

        /// <inheritdoc />
        public async Task<List<GamePositionSummary>> GetGamePositionSummariesForGame(int gameId)
        {
            using var connection = GetOpenConnection();
            var list = (await connection.QueryAsync<GamePositionSummary>(
                SqlStatements.GetGamePositionSummariesForGame,
                new { GameId = gameId })).ToList();

            return list;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<MaterialAverageByYearRow>> GetMaterialAveragesByYearAtPlyAsync(
            AnalyticsQuery query,
            int plyIndex,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<MaterialAverageByYearRow>(
                new CommandDefinition(
                    SqlStatements.GetMaterialAveragesByYearAtPly,
                    new
                    {
                        PlyIndex = plyIndex,
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = query.Eco
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<KnightDestinationCountRow>> GetKnightDestinationCountsAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<KnightDestinationCountRow>(
                new CommandDefinition(
                    SqlStatements.GetKnightDestinationCounts,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = query.Eco
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<GameCountByEcoRow>> GetGameCountsByEcoAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<GameCountByEcoRow>(
                new CommandDefinition(
                    SqlStatements.GetGameCountsByEco,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<GameCountByYearRow>> GetGameCountsByYearAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<GameCountByYearRow>(
                new CommandDefinition(
                    SqlStatements.GetGameCountsByYear,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<GameCountByResultRow>> GetGameCountsByResultAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<GameCountByResultRow>(
                new CommandDefinition(
                    SqlStatements.GetGameCountsByResult,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<GameCountByPlayerRow>> GetGameCountsByPlayerAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<GameCountByPlayerRow>(
                new CommandDefinition(
                    SqlStatements.GetGameCountsByPlayer,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PlayerResultSummaryRow>> GetPlayerResultSummariesAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<PlayerResultSummaryRow>(
                new CommandDefinition(
                    SqlStatements.GetPlayerResultSummaries,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PlayerMaterialAverageRow>> GetPlayerMaterialAveragesAtPlyAsync(
            AnalyticsQuery query,
            int moveNumber,
            int plyIndex,
            string colourMode,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<PlayerMaterialAverageRow>(
                new CommandDefinition(
                    SqlStatements.GetPlayerMaterialAveragesAtPly,
                    new
                    {
                        MoveNumber = moveNumber,
                        PlyIndex = plyIndex,
                        ColourMode = colourMode,
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerASurname = NormalizeNonEmpty(query.PlayerASurname),
                        PlayerAForenames = NormalizeNamePart(query.PlayerAForenames),
                        PlayerBSurname = NormalizeNonEmpty(query.PlayerBSurname),
                        PlayerBForenames = NormalizeNamePart(query.PlayerBForenames),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<AverageCastlingPlyRow>> GetAverageCastlingPlyAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<AverageCastlingPlyRow>(
                new CommandDefinition(
                    SqlStatements.GetAverageCastlingPly,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerAverageCastlingPlyAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<PlayerStylePerPlayerMetricRow>(
                new CommandDefinition(
                    SqlStatements.GetPerPlayerAverageCastlingPly,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<AverageMaterialVolatilityRow>> GetAverageMaterialVolatilityAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<AverageMaterialVolatilityRow>(
                new CommandDefinition(
                    SqlStatements.GetAverageMaterialVolatility,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco),
                        MinPlyIndex = query.MinPlyIndex,
                        MaxPlyIndex = query.MaxPlyIndex
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerAverageMaterialVolatilityAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<PlayerStylePerPlayerMetricRow>(
                new CommandDefinition(
                    SqlStatements.GetPerPlayerAverageMaterialVolatility,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco),
                        MinPlyIndex = query.MinPlyIndex,
                        MaxPlyIndex = query.MaxPlyIndex
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<BishopPairFrequencyRow>> GetBishopPairFrequencyAsync(
            AnalyticsQuery query,
            int? minPlyIndex,
            int? maxPlyIndex,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<BishopPairFrequencyRow>(
                new CommandDefinition(
                    SqlStatements.GetBishopPairFrequency,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco),
                        MinPlyIndex = minPlyIndex,
                        MaxPlyIndex = maxPlyIndex
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerBishopPairFrequencyAsync(
            AnalyticsQuery query,
            int? minPlyIndex,
            int? maxPlyIndex,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<PlayerStylePerPlayerMetricRow>(
                new CommandDefinition(
                    SqlStatements.GetPerPlayerBishopPairFrequency,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco),
                        MinPlyIndex = minPlyIndex,
                        MaxPlyIndex = maxPlyIndex
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<MinorPieceCompositionRow>> GetMinorPieceCompositionAsync(
            AnalyticsQuery query,
            int? minPlyIndex,
            int? maxPlyIndex,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<MinorPieceCompositionRow>(
                new CommandDefinition(
                    SqlStatements.GetMinorPieceComposition,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco),
                        MinPlyIndex = minPlyIndex,
                        MaxPlyIndex = maxPlyIndex
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerMinorPieceCompositionAsync(
            AnalyticsQuery query,
            int? minPlyIndex,
            int? maxPlyIndex,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<PlayerStylePerPlayerMetricRow>(
                new CommandDefinition(
                    SqlStatements.GetPerPlayerMinorPieceComposition,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco),
                        MinPlyIndex = minPlyIndex,
                        MaxPlyIndex = maxPlyIndex
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<CaptureRateRow>> GetCaptureRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<CaptureRateRow>(
                new CommandDefinition(
                    SqlStatements.GetCaptureRate,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco),
                        MinPlyIndex = query.MinPlyIndex,
                        MaxPlyIndex = query.MaxPlyIndex
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerCaptureRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<PlayerStylePerPlayerMetricRow>(
                new CommandDefinition(
                    SqlStatements.GetPerPlayerCaptureRate,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco),
                        MinPlyIndex = query.MinPlyIndex,
                        MaxPlyIndex = query.MaxPlyIndex
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<QueenTradeRateRow>> GetQueenTradeRateAsync(
            AnalyticsQuery query,
            int queenTradeMaxPly,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<QueenTradeRateRow>(
                new CommandDefinition(
                    SqlStatements.GetQueenTradeRate,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco),
                        QueenTradeMaxPly = queenTradeMaxPly
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerQueenTradeRateAsync(
            AnalyticsQuery query,
            int queenTradeMaxPly,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<PlayerStylePerPlayerMetricRow>(
                new CommandDefinition(
                    SqlStatements.GetPerPlayerQueenTradeRate,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco),
                        QueenTradeMaxPly = queenTradeMaxPly
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<CentreMoveRateRow>> GetCentreMoveRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<CentreMoveRateRow>(
                new CommandDefinition(
                    SqlStatements.GetCentreMoveRate,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco),
                        MinPlyIndex = query.MinPlyIndex,
                        MaxPlyIndex = query.MaxPlyIndex
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerCentreMoveRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<PlayerStylePerPlayerMetricRow>(
                new CommandDefinition(
                    SqlStatements.GetPerPlayerCentreMoveRate,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco),
                        MinPlyIndex = query.MinPlyIndex,
                        MaxPlyIndex = query.MaxPlyIndex
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ForwardMoveRateRow>> GetForwardMoveRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<ForwardMoveRateRow>(
                new CommandDefinition(
                    SqlStatements.GetForwardMoveRate,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco),
                        MinPlyIndex = query.MinPlyIndex,
                        MaxPlyIndex = query.MaxPlyIndex
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerForwardMoveRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<PlayerStylePerPlayerMetricRow>(
                new CommandDefinition(
                    SqlStatements.GetPerPlayerForwardMoveRate,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco),
                        MinPlyIndex = query.MinPlyIndex,
                        MaxPlyIndex = query.MaxPlyIndex
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<CastlingSidePreferenceRow>> GetCastlingSidePreferenceAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<CastlingSidePreferenceRow>(
                new CommandDefinition(
                    SqlStatements.GetCastlingSidePreference,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerCastlingSidePreferenceAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<PlayerStylePerPlayerMetricRow>(
                new CommandDefinition(
                    SqlStatements.GetPerPlayerCastlingSidePreference,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<OppositeSideCastlingRateRow>> GetOppositeSideCastlingRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<OppositeSideCastlingRateRow>(
                new CommandDefinition(
                    SqlStatements.GetOppositeSideCastlingRate,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerOppositeSideCastlingRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<PlayerStylePerPlayerMetricRow>(
                new CommandDefinition(
                    SqlStatements.GetPerPlayerOppositeSideCastlingRate,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<UncastledKingRateRow>> GetUncastledKingRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<UncastledKingRateRow>(
                new CommandDefinition(
                    SqlStatements.GetUncastledKingRate,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerUncastledKingRateAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<PlayerStylePerPlayerMetricRow>(
                new CommandDefinition(
                    SqlStatements.GetPerPlayerUncastledKingRate,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<FirstQueenMovePlyRow>> GetFirstQueenMovePlyAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<FirstQueenMovePlyRow>(
                new CommandDefinition(
                    SqlStatements.GetFirstQueenMovePly,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerFirstQueenMovePlyAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<PlayerStylePerPlayerMetricRow>(
                new CommandDefinition(
                    SqlStatements.GetPerPlayerFirstQueenMovePly,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<AverageGameLengthRow>> GetAverageGameLengthAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<AverageGameLengthRow>(
                new CommandDefinition(
                    SqlStatements.GetAverageGameLength,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerSurname = NormalizeNonEmpty(query.PlayerSurname),
                        PlayerForenames = NormalizeNamePart(query.PlayerForenames),
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PlayerStylePerPlayerMetricRow>> GetPerPlayerAverageGameLengthAsync(
            AnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<PlayerStylePerPlayerMetricRow>(
                new CommandDefinition(
                    SqlStatements.GetPerPlayerAverageGameLength,
                    new
                    {
                        MinGameYear = query.MinGameYear,
                        MaxGameYear = query.MaxGameYear,
                        PlayerColour = NormalizePlayerColourFilter(query.PlayerColour),
                        Eco = NormalizeNonEmpty(query.Eco)
                    },
                    cancellationToken: cancellationToken))).ToList();

            return rows;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<int>> GetGameIdsNeedingAnalyticsBackfillAsync(CancellationToken cancellationToken = default)
        {
            using var connection = GetOpenConnection();
            return (await connection.QueryAsync<int>(
                new CommandDefinition(
                    SqlStatements.GetGameIdsNeedingAnalyticsBackfill,
                    cancellationToken: cancellationToken))).ToList();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<(int PlyIndex, BoardPosition Position)>> GetBoardPositionsForGameOrderedAsync(
            int gameId,
            CancellationToken cancellationToken = default)
        {
            using var connection = GetOpenConnection();
            var rows = (await connection.QueryAsync<BoardPositionDbRow>(
                new CommandDefinition(
                    SqlStatements.GetBoardPositionsForGameOrdered,
                    new { GameId = gameId },
                    cancellationToken: cancellationToken))).ToList();

            return rows.Select(r => (r.PlyIndex, MapBoardPosition(r))).ToList();
        }

        private static BoardPosition MapBoardPosition(BoardPositionDbRow r)
        {
            var bp = new BoardPosition();
            bp.PiecePositions["WP"] = (ulong)r.WP;
            bp.PiecePositions["WN"] = (ulong)r.WN;
            bp.PiecePositions["WB"] = (ulong)r.WB;
            bp.PiecePositions["WR"] = (ulong)r.WR;
            bp.PiecePositions["WQ"] = (ulong)r.WQ;
            bp.PiecePositions["WK"] = (ulong)r.WK;
            bp.PiecePositions["BP"] = (ulong)r.BP;
            bp.PiecePositions["BN"] = (ulong)r.BN;
            bp.PiecePositions["BB"] = (ulong)r.BB;
            bp.PiecePositions["BR"] = (ulong)r.BR;
            bp.PiecePositions["BQ"] = (ulong)r.BQ;
            bp.PiecePositions["BK"] = (ulong)r.BK;

            if (!string.IsNullOrWhiteSpace(r.EnPassantTargetFile))
                bp.EnPassantTargetFile = char.ToUpperInvariant(r.EnPassantTargetFile.Trim()[0]);

            return bp;
        }

        private static GameMoveFact Map(GameMoveRow r) =>
            new()
            {
                GameId = r.GameId,
                PlyIndex = r.PlyIndex,
                MovingSide = string.IsNullOrEmpty(r.MovingSide) ? '\0' : r.MovingSide[0],
                FromSquare = r.FromSquare,
                ToSquare = r.ToSquare,
                MovedPiece = string.IsNullOrEmpty(r.MovedPiece) ? '\0' : r.MovedPiece[0],
                CapturedPiece = string.IsNullOrEmpty(r.CapturedPiece) ? null : r.CapturedPiece[0],
                PromotionPiece = string.IsNullOrEmpty(r.PromotionPiece) ? null : r.PromotionPiece[0],
                IsCastlingKingside = r.IsCastlingKingside,
                IsCastlingQueenside = r.IsCastlingQueenside
            };

        private sealed class BoardPositionDbRow
        {
            public int PlyIndex { get; set; }

            public long WP { get; set; }

            public long WN { get; set; }

            public long WB { get; set; }

            public long WR { get; set; }

            public long WQ { get; set; }

            public long WK { get; set; }

            public long BP { get; set; }

            public long BN { get; set; }

            public long BB { get; set; }

            public long BR { get; set; }

            public long BQ { get; set; }

            public long BK { get; set; }

            public string? EnPassantTargetFile { get; set; }
        }

        private sealed class GameMoveRow
        {
            public int GameId { get; set; }

            public int PlyIndex { get; set; }

            public string MovingSide { get; set; } = "";

            public byte FromSquare { get; set; }

            public byte ToSquare { get; set; }

            public string MovedPiece { get; set; } = "";

            public string? CapturedPiece { get; set; }

            public string? PromotionPiece { get; set; }

            public bool IsCastlingKingside { get; set; }

            public bool IsCastlingQueenside { get; set; }
        }
    }
}
