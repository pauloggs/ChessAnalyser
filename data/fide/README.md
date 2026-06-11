# FIDE rating list files (local only)

ChessAnalyser does **not** commit official FIDE rating list downloads. Obtain a list file yourself and keep it outside git (or in this folder, which is gitignored except this README).

## Download

1. Open [ratings.fide.com/download_lists.phtml](https://ratings.fide.com/download_lists.phtml)
2. Download **Combined list STD, BLZ, RPD** in **TXT** format (or the Standard-only TXT if you prefer a smaller file)
3. Save the extracted `.txt` file here, for example:
   - `data/fide/players_list_foa.txt`

The file is **not** in git (`data/fide/*` is gitignored). Until you download and save it, import will fail with `FileNotFoundException`.

Paths like `data/fide/players_list_foa.txt` are resolved from the **repo root** when you run `dotnet run --project src/Analyser` from the repo root. Absolute paths also work.

## Import into Ref.FidePlayer

The rating list is loaded into **`Ref.FidePlayer`** (migration `014`), then corpus players are backfilled automatically:

```powershell
dotnet run --project src/Analyser -- --import-fide-catalog data/fide/players_list_foa.txt
```

Optional `--dry-run` previews matches without writing.

Re-run when you download a newer monthly list — import replaces the catalog and backfills changed rows.

## During PGN load

After the catalog is imported once, **new players** created during ETL receive FIDE metadata automatically (when a confident name match exists in `Ref.FidePlayer`).

To refresh all existing players without re-importing the file:

```powershell
dotnet run --project src/Analyser -- --sync-player-metadata
```

## Format

FIDE publishes a **fixed-width** text file. The reader (`FideRatingListReader`) parses:

| Field | Notes |
|-------|--------|
| FIDE ID | First 15 columns |
| Name | `Surname, Forenames` in columns 16–75 |
| Federation | 3-letter code |
| Sex | `M` or `F` |
| Title | `GM`, `IM`, etc. in the title region |
| Birth year | Last 4-digit year token on the line (before optional inactive flag) |

## Licence

Data © FIDE. Use subject to [FIDE's terms](https://ratings.fide.com/). This project stores **no** FIDE files in the repository.
