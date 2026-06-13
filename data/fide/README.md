# FIDE rating list file (local, gitignored)

Place your official FIDE combined list here:

- `data/fide/players_list_foa.txt`

Download from [ratings.fide.com/download_lists.phtml](https://ratings.fide.com/download_lists.phtml) (**Combined list STD, BLZ, RPD**, TXT format).

## Load catalog

### Command line

From the repo root (with the file in place):

```bash
dotnet run --project src/Analyser -- --seed-fide-catalog
```

Reload after `TRUNCATE` or to refresh the monthly list:

```bash
dotnet run --project src/Analyser -- --seed-fide-catalog --force
```

Optional custom path:

```bash
dotnet run --project src/Analyser -- --seed-fide-catalog --force --path "D:\data\players_list_foa.txt"
```

Seeding runs only when `Ref.FidePlayer` is empty unless `--force` is passed. `--force` clears `App.Player.FidePlayerId` links before replacing catalog rows (~1.8M rows; may take several minutes).

### Web UI

Open the app home page (`https://localhost:5001/`), section **Player metadata**:

1. **Seed FIDE catalog** — `POST /Analyser/SeedFideCatalog` (poll `GET /Analyser/MaintenanceProgress`)
2. **Link player metadata** — `POST /Analyser/LinkPlayerMetadata` (same progress endpoint)

Status: `GET /Analyser/FideCatalogStatus` (row count + whether the configured file exists).

## Link players after seed

```bash
dotnet run --project src/Analyser -- --link-player-metadata
```

Sets `App.Player.WorldChampionId` and `FidePlayerId` from `Ref.*` via C# matchers.

## Configuration

`src/Analyser/appsettings.json`:

```json
"FideCatalog": {
  "ListPath": "data/fide/players_list_foa.txt"
}
```

`ListPath` is resolved **relative to the repository root** (directory containing `ChessAnalyser.sln`), not the Analyser project folder.

Override with `--path` (CLI) or `ListPath` in the UI seed request body.

## Format

Fixed-width FIDE TXT — parsed by `FideRatingListReader` (see DESIGN §13.5).

## Licence

Data © FIDE. Use subject to [FIDE's terms](https://ratings.fide.com/).
