# ChessAnalyser Database Migrations (DbUp)

Runs SQL scripts from `Scripts/` against the Chess database. Each script runs **once** and is recorded in `dbo.schemaversions`.

DbUp does **not** re-apply a script when the file changes. To fix a database that already ran a script, adjust schema manually or reset the journal and affected objects.

## Run

```bash
dotnet run --project src/Migrations/Migrations.csproj
```

## Connection string

- `appsettings.json` → `ConnectionStrings:ChessConnection`
- Override: `ConnectionStrings__ChessConnection` environment variable

## Scripts (001–013)

| Script | Purpose |
|--------|---------|
| `001`–`010` | Core game, position, and player schema (created in `dbo`) |
| `004` | `dbo.Player` — `Id`, `Surname`, `Forenames` |
| `011` | `Ref.WorldChampion` (drop/recreate + seed 18 rows) + `dbo.Player.WorldChampionId` FK |
| `012` | `Ref.FidePlayer` (create if missing) + `dbo.Player.FidePlayerId` FK |
| `013` | `App` schema — transfer operational tables from `dbo`; recreate `App.DeleteGameById` |

**Schemas:** `App.*` — games, players, positions, analytics facts. `Ref.*` — reference catalogs (FIDE, world champions). `dbo.SchemaVersions` — DbUp journal (unchanged).

`App.Player` holds PGN identity plus nullable FKs to `Ref.*` only (3NF).

Name matching (PGN variants, homonyms) is implemented in **C#**, not SQL alias tables.

## Schema history

After schema changes on a live database:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\export-db-history.ps1
```
