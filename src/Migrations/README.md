# ChessAnalyser Database Migrations (DbUp)

This console app runs SQL migration scripts against the Chess database. It uses [DbUp](https://dbup.readthedocs.io/): each script runs **once** and is recorded in `dbo.schemaversions`.

**Important:** DbUp does not re-apply a script when the `.sql` file changes. If a migration was wrong, editing the file does not fix databases that already ran it. Use [tools/RebuildWorldChampionRef.sql](../../tools/RebuildWorldChampionRef.sql) for world-champion recovery.

## Running migrations

From the repo root:

```bash
dotnet run --project src/Migrations/Migrations.csproj
```

## Connection string

- **Default:** `appsettings.json` → `ConnectionStrings:ChessConnection` (copied to build output).
- **Override:** `ConnectionStrings__ChessConnection` environment variable.

| In migrations | In application |
|---------------|----------------|
| Schema + `Ref.*` seed | `IPlayerFideMetadataEnricher` links `dbo.Player` |

**Never** put `UPDATE dbo.Player` backfill in SQL migrations.

## World-champion recovery (broken Ref.WorldChampion)

If constraints are missing or data is wrong:

```cmd
sqlcmd -S 127.0.0.1 -U SA -P "<pw>" -C -i tools\RebuildWorldChampionRef.sql
dotnet run --project src/Migrations/Migrations.csproj
```

`RebuildWorldChampionRef.sql` will:

1. Keep all `dbo.Player` rows; clear metadata columns only  
2. Keep `Ref.FidePlayer` untouched  
3. **Drop and recreate** WC tables with **all four constraints**  
4. Seed 18 champions (`Id` = `ChampionOrder`) 
5. Recreate `WorldChampionId` on `dbo.Player`  
6. **Verify** constraints (fails if any missing)  
7. Mark `013`–`017` as applied in `schemaversions`  

Then `dotnet run` only runs enrichment.

Verify:

```sql
SELECT name, type_desc FROM sys.objects
WHERE parent_object_id = OBJECT_ID(N'Ref.WorldChampion') AND type IN ('C','UQ','PK');
-- CK_Ref_WorldChampion_Id_ChampionOrder
-- PK_Ref_WorldChampion
-- UQ_Ref_WorldChampion_ChampionOrder
-- UQ_Ref_WorldChampion_Surname_Forenames
```

## Schema history

Regenerate `src/Migrations/History/current/` after schema changes:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\export-db-history.ps1
```
