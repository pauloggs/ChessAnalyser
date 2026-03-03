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
|--------|--------|
| `001_CreateGameTable.sql` | Creates `dbo.Game` (Id, Name, GameId, Winner) if missing. |
| `002_CreateBoardPositionTable.sql` | Creates `dbo.BoardPosition` (GameId, PlyIndex, 12 bitboard columns, EnPassantTargetFile) if missing. |

`BoardPosition` uses `PlyIndex`: **-1** = initial position, **0, 1, 2, ...** = position after each ply. Columns `WP`, `WN`, … `BK` store 64-bit bitboards as `BIGINT`.
