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

`BoardPosition` uses `PlyIndex`: **-1** = initial position, **0, 1, 2, ...** = position after each ply. Columns `WP`, `WN`, … `BK` store 64-bit bitboards as `BIGINT`.

`Game` analytics columns are populated by application code in a later change (see `docs/PLAN.md` §11); until then they remain NULL for existing rows.

## Schema history snapshot

`src/Migrations/History/current/` contains a generated snapshot of current SQL object definitions (tables, views, procedures, functions, and user-defined table types).

Generate/update it from repo root:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\export-db-history.ps1
```

The script reads `ConnectionStrings:ChessConnection` from `src/Migrations/appsettings.json` by default, or accepts an explicit `-ConnectionString`.

Whenever migrations change, regenerate `History/current` and commit those files in the same PR so migration intent and resulting schema stay aligned.
