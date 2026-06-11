# FIDE rating list files (local only)

ChessAnalyser does **not** commit official FIDE rating list downloads. Obtain a list file yourself and keep it outside git (or in this folder, which is gitignored except this README).

## Download

1. Open [ratings.fide.com/download_lists.phtml](https://ratings.fide.com/download_lists.phtml)
2. Download **Combined list STD, BLZ, RPD** in **TXT** format (or the Standard-only TXT if you prefer a smaller file)
3. Save the extracted `.txt` file here, for example:
   - `data/fide/players_list_foa.txt`

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

## Usage (after PLAN §15.3 lands)

```powershell
dotnet run --project src/Analyser -- --sync-fide-metadata data/fide/players_list_foa.txt
```

Until the sync CLI exists, the reader and matcher are covered by unit tests only.

## Licence

Data © FIDE. Use subject to [FIDE's terms](https://ratings.fide.com/). This project stores **no** FIDE files in the repository.
