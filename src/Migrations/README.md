# ChessAnalyser Database Migrations (DbUp)

This console app runs SQL migration scripts against the Chess database. It uses [DbUp](https://dbup.readthedocs.io/): each script runs once and is recorded in `dbo.schemaversions`.

## Running migrations

From the repo root:

```bash
dotnet run --project src/Migrations/Migrations.csproj
```

Or from the Migrations project directory:

```bash
dotnet run
```

## Connection string

- **Default:** Read from `appsettings.json` → `ConnectionStrings:ChessConnection`.
- **Override:** Set the same key via environment variable (e.g. `ConnectionStrings__ChessConnection`) or change `appsettings.json` for local use.

Ensure the **database** (e.g. `Chess`) already exists; DbUp does not create it. The scripts create tables idempotently (`IF OBJECT_ID ... IS NULL`).

## Scripts (run in filename order)

| Script | Purpose |
|--------|---------|
| `001_CreateGameTable.sql` | Creates `dbo.Game` (Id, Name, GameId, Winner) if missing. |
| `002_CreateBoardPositionTable.sql` | Creates `dbo.BoardPosition` (GameId, PlyIndex, 12 bitboard columns, EnPassantTargetFile) if missing. |
| `003_CreateGameParseErrorTable.sql` | Creates `dbo.GameParseError` for parse error logging. |
| `004_CreatePlayerTable.sql` | Creates `dbo.Player` (Surname, Forenames). |
| `005_AddPlayerRefsToGame.sql` | Adds `WhitePlayerId`, `BlackPlayerId` FKs to `dbo.Game`. |
| `006_AddGameAnalyticsColumns.sql` | Adds nullable analytics columns on `dbo.Game`: `Event`, `Site`, `DateTag`, `GameYear`, `Eco`; filtered indexes on `GameYear` and `Eco`. |
| `007_CreateGameMoveTable.sql` | Creates `dbo.GameMove` secondary fact table (one row per ply) with side/from/to/piece flags and an index for destination-piece frequency queries. |
| `008_CreateGamePositionSummaryTable.sql` | Creates `dbo.GamePositionSummary` rollup table (material + piece-count scalars per ply) for analytics reads without bitboard decoding. |
| `009_CreateDeleteGameStoredProcedure.sql` | Creates `dbo.DeleteGameById` to delete a game and dependent rows in one explicit transaction (instead of FK cascades on analytics tables). |
| `010_RemoveCascadeDeletesFromGameDependencies.sql` | Enforces strict no-cascade FKs from `BoardPosition`, `GameMove`, and `GamePositionSummary` to `Game` (drops/recreates FK if cascade is present). |

`BoardPosition` uses `PlyIndex`: **-1** = initial position, **0, 1, 2, ...** = position after each ply. Columns `WP`, `WN`, … `BK` store 64-bit bitboards as `BIGINT`.

`Game` analytics columns are populated by application code before insert (`PgnGameHeaderMapper` in `Services`); older rows may still have NULL values unless backfilled.

`GameMove` and `GamePositionSummary` are created in schema first and are intended to be populated by a follow-up analytics materialization step in application code (see `docs/PLAN.md` §11).

## Schema history snapshot

`src/Migrations/History/current/` contains a generated snapshot of current SQL object definitions (tables, views, procedures, functions, and user-defined table types).

**CI:** the “Schema history” GitHub workflow uses `SchemaHistoryExporter`; when you refresh `History/current` for a PR, regenerate with that project too so output matches CI.

Generate/update it from repo root:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\export-db-history.ps1
```

The script reads `ConnectionStrings:ChessConnection` from `src/Migrations/appsettings.json` by default, or accepts an explicit `-ConnectionString`.

Whenever migrations change, regenerate `History/current` and commit those files in the same PR so migration intent and resulting schema stay aligned.
