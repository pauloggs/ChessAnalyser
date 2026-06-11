# FIDE rating list file (local, gitignored)

ChessAnalyser does **not** commit the official FIDE download. Place your list here:

- `data/fide/players_list_foa.txt`

Download from [ratings.fide.com/download_lists.phtml](https://ratings.fide.com/download_lists.phtml) (**Combined list STD, BLZ, RPD**, TXT format).

## Automatic load (no manual import commands)

When you run the normal database migrations:

```text
dotnet run --project src/Migrations
```

the Migrations host will:

1. Apply SQL scripts through `015_SeedRefFidePlayer.sql`
2. If `Ref.FidePlayer` is empty, stream `data/fide/players_list_foa.txt` into the catalog (~1.8M rows; may take several minutes)
3. Backfill `dbo.Player` FIDE metadata and world-champion flags from `Ref.*` tables

Override the file path in `src/Migrations/appsettings.json`:

```json
"FideCatalog": { "ListPath": "data/fide/players_list_foa.txt" }
```

## During PGN load

Each persisted game enriches both players using that game's **GameYear**. At the end of every load, a full idempotent enrichment pass runs automatically. No manual sync commands.

## Date verification

A FIDE candidate is rejected when their birth year is **after any game year** in the corpus (e.g. birth 1983 cannot match a player whose games start in 1925). Wrong assignments are cleared on the next enrichment pass.

## Refreshing the catalog

Seeding runs only when `Ref.FidePlayer` is empty. To load a newer monthly list: `TRUNCATE TABLE Ref.FidePlayer;` then run migrations again.

## Format

Fixed-width FIDE TXT — parsed by `FideRatingListReader` (see DESIGN §13.5).

## Licence

Data © FIDE. Use subject to [FIDE's terms](https://ratings.fide.com/).
