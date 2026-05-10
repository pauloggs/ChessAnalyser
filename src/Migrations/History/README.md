# SQL object history (`History/current`)

This folder stores the **current canonical DDL snapshot** for SQL objects after migrations are applied.

## Why

Migration scripts are append-only and great for change intent, but not ideal for quickly answering "what does the database look like now?".

`History/current` solves this by exporting one `.sql` file per object, grouped by object type.

## Structure

- `current/tables/`
- `current/views/`
- `current/procedures/`
- `current/functions/`
- `current/types/` (user-defined table types)
- `releases/` (optional point-in-time snapshots)

## Regenerate the current snapshot

From repo root:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\export-db-history.ps1
```

Optional explicit connection:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\export-db-history.ps1 -ConnectionString "Server=...;Database=...;..."
```

By default, the script reads `ConnectionStrings:ChessConnection` from `src/Migrations/appsettings.json`.

## Dependency

The exporter uses SMO via the PowerShell module `SqlServer`.
Install once if needed:

```powershell
Install-Module SqlServer -Scope CurrentUser
```

## Team convention

Whenever a migration script changes (`src/Migrations/Scripts/*.sql`):

1. Apply migrations.
2. Regenerate `History/current`.
3. Commit migration + history diff in the same PR.

This keeps migration intent and resulting schema in sync.
