# ChessAnalyser - Example analyses

**Purpose:** A living cookbook for analyses that can be run against the local ChessAnalyser database.
Keep this document practical: each entry should say what question it answers, what data must exist,
how to run it, and how to interpret the result.

---

## 1. Before running analyses

Use the same SQL Server database configured as `ConnectionStrings:ChessConnection` for the
`Analyser` project.

Minimum data prerequisites:

- Migrations have run through the current scripts.
- `dbo.Game` contains freshly loaded rows with metadata such as `Event`, `Site`, `DateTag`,
  `GameYear`, and `Eco`.
- `dbo.BoardPosition` contains the per-ply board positions for those games.
- `dbo.GameMove` and `dbo.GamePositionSummary` are populated.

Fresh PGN loads populate analytics rows automatically after each game insert. Older games that
already had `BoardPosition` rows but no analytics rows need a backfill:

```bash
dotnet run --project src/Analyser/Analyser.csproj -- --backfill-analytics
```

For a smaller trial run:

```bash
dotnet run --project src/Analyser/Analyser.csproj -- --backfill-analytics --max-games 100
```

If you have just cleaned the database and reloaded PGNs, spot-check that header metadata is present:

```sql
SELECT TOP 20
    Id,
    Name,
    Event,
    Site,
    DateTag,
    GameYear,
    Eco,
    WhitePlayerId, -- storage FK; UI/API filters use player surname/forenames
    BlackPlayerId
FROM dbo.[Game]
ORDER BY Id DESC;
```

Spot-check analytics row coverage:

```sql
SELECT
    (SELECT COUNT(*) FROM dbo.[BoardPosition]) AS BoardPositionRows,
    (SELECT COUNT(*) FROM dbo.[GamePositionSummary]) AS GamePositionSummaryRows,
    (SELECT COUNT(*) FROM dbo.[GameMove]) AS GameMoveRows;
```

For successfully materialized games, `GamePositionSummary` should be roughly one row per
`BoardPosition`. `GameMove` should be roughly one row fewer per game because moves are derived from
consecutive position pairs.

---

## 2. Running from the local web UI

Start the host:

```bash
dotnet run --project src/Analyser/Analyser.csproj
```

Open the local UI:

- `https://localhost:5001/`
- or `http://localhost:5000/`

Use the page sections:

- **PGN load** - load PGN files, watch progress, cancel if needed.
- **Analytics metrics** - discover registered metrics, set filters, and execute them.
- **Games (paged)** - browse `Game` rows with paging and filters.
- **API docs (Swagger)** - optional API reference; the web UI is the primary daily workflow.

Do not open `wwwroot/index.html` directly with `file://`; the UI calls relative API paths on the
same host.

---

## 3. Current registered metrics

Discover metrics from the web UI or by calling:

```http
GET /api/analytics/metrics
```

Current metric keys:

- `AverageMaterialByYearAndColour`
- `KnightMoveDestinationFrequency`
- `GameCountByEco`

Metrics support the shared `AnalyticsQuery` filter shape where the filter is meaningful for that
metric:

```json
{
  "minGameYear": 1900,
  "maxGameYear": 1950,
  "whitePlayerSurname": null,
  "whitePlayerForenames": null,
  "blackPlayerSurname": null,
  "blackPlayerForenames": null,
  "eco": null,
  "summaryPlyIndex": 4
}
```

Omit properties, or leave them blank in the UI, when you do not want that filter.

---

## 4. Example analysis: average side material by year

### Question

How does average material for the White side and Black side vary by year at an early fixed ply?

### Why it is useful

This is a corpus-level board-state sanity check and a starting point for historical comparisons. At
a low ply such as `4`, most games should still be near starting material unless early captures
happened. Large or surprising differences can point to data issues, unusual PGN parsing, or
interesting opening patterns.

This metric compares **sides in the selected game set**: average `WhiteMaterial` for those games
and average `BlackMaterial` for those same games. It is not an independent player-comparison metric.

### Run it in the UI

In **Analytics metrics**:

- Metric key: `AverageMaterialByYearAndColour`
- `summaryPlyIndex`: `4` (or leave empty for the default)
- Optional: set `minGameYear`, `maxGameYear`, or `eco`

Click **Run metric**.

### Equivalent HTTP request

```http
POST /api/analytics/metrics/execute
Content-Type: application/json
```

```json
{
  "metricKey": "AverageMaterialByYearAndColour",
  "query": {
    "minGameYear": 1900,
    "maxGameYear": 1950,
    "summaryPlyIndex": 4
  }
}
```

### Result columns

- `GameYear`
- `AvgWhiteMaterial`
- `AvgBlackMaterial`
- `GameCount`

### Notes

- Games with `GameYear = NULL` are excluded by year-based metrics.
- `summaryPlyIndex = 4` means the board snapshot after ply 4 using this repo's ply convention.
- Material uses the configured `IPieceValues` implementation (`ClassicalPieceValues` today).
- Player filters only narrow the game set before the side averages are calculated. For example,
  filtering White player to `Kasparov, Garry` compares Kasparov's White-side material with the
  Black-side material of his opponents in those same games. It does **not** compare Kasparov against
  an all-player baseline or against another player independently.
- Use the planned player-comparison metric (PLAN §12.5) for player-vs-player or player-vs-baseline
  material questions.

---

## 5. Example analysis: knight destination frequency

### Question

Which destination squares do knights move to most often, optionally filtered by year, ECO, or
player?

### Why it is useful

This is a compact move-frequency analysis over the derived `GameMove` fact table. It is useful for
checking whether move derivation is working and for exploring opening or player tendencies.

### Run it in the UI

In **Analytics metrics**:

- Metric key: `KnightMoveDestinationFrequency`
- Optional: set `minGameYear`, `maxGameYear`, `eco`, or choose White/Black players by name

Click **Run metric**.

### Equivalent HTTP request

```http
POST /api/analytics/metrics/execute
Content-Type: application/json
```

```json
{
  "metricKey": "KnightMoveDestinationFrequency",
  "query": {
    "minGameYear": 1900,
    "maxGameYear": 1950,
    "eco": "B90"
  }
}
```

### Result columns

- `ToSquare`
- `MoveCount`

### Notes

- `ToSquare` is the numeric square index used by the application (`0` to `63`, with `a1 = 0`).
- A future UI improvement could render algebraic labels (`e4`, `f3`, etc.) beside the numeric
  square.
- This metric depends on `dbo.GameMove`, so it will be empty for games that have not been
  materialized or backfilled.

---

## 6. Example analysis: game count by ECO

### Question

Which ECO codes occur most often in the loaded game set?

### Why it is useful

This is a fast corpus-shape metric. It helps you see which openings dominate the data and is easy
to verify against `dbo.Game`.

### Run it in the UI

In **Analytics metrics**:

- Metric key: `GameCountByEco`
- Optional: set `minGameYear`, `maxGameYear`, `eco`, or choose White/Black players by name

Click **Run metric**.

### Equivalent HTTP request

```http
POST /api/analytics/metrics/execute
Content-Type: application/json
```

```json
{
  "metricKey": "GameCountByEco",
  "query": {
    "minGameYear": 1900,
    "maxGameYear": 1950
  }
}
```

### Result columns

- `Eco`
- `GameCount`

### Notes

- Games with missing or blank `Eco` are excluded.
- Supplying `eco` narrows the result to that exact code, which is mostly useful as a quick check.

---

## 7. Example analysis: browse the game population

### Question

What games are currently in the database, and do they have the expected metadata?

### Run it in the UI

In **Games (paged)**:

- `page`: `1`
- `pageSize`: `50`
- Optional filters: `minGameYear`, `maxGameYear`, White/Black player names, `eco`

Click **Fetch page**.

### Equivalent HTTP request

```http
GET /Analyser/GetGames?page=1&pageSize=50&minGameYear=1900&maxGameYear=1950
```

### What to look for

- `event`, `site`, `dateTag`, `gameYear`, and `eco` are populated where the PGN has those tags.
- White/Black player names in the UI are populated from the resolved `Player` rows.
- Page counts look reasonable for your loaded PGN set.

---

## 8. Useful SQL checks

Use these as diagnostics, not as the primary application surface.

Find games missing metadata after a reload:

```sql
SELECT TOP 50 Id, Name, Event, Site, DateTag, GameYear, Eco
FROM dbo.[Game]
WHERE Event IS NULL
   OR DateTag IS NULL
   OR GameYear IS NULL
ORDER BY Id;
```

Count games by year:

```sql
SELECT GameYear, COUNT(*) AS GameCount
FROM dbo.[Game]
GROUP BY GameYear
ORDER BY GameYear;
```

Check analytics rows per game for a small sample:

```sql
SELECT TOP 25
    g.Id,
    g.Name,
    COUNT(DISTINCT bp.PlyIndex) AS BoardPositions,
    COUNT(DISTINCT gps.PlyIndex) AS PositionSummaries,
    COUNT(DISTINCT gm.PlyIndex) AS Moves
FROM dbo.[Game] g
LEFT JOIN dbo.[BoardPosition] bp ON bp.GameId = g.Id
LEFT JOIN dbo.[GamePositionSummary] gps ON gps.GameId = g.Id
LEFT JOIN dbo.[GameMove] gm ON gm.GameId = g.Id
GROUP BY g.Id, g.Name
ORDER BY g.Id;
```

---

## 9. Adding a new example analysis to this document

Use this template:

```markdown
## Example analysis: <name>

### Question

<What does this answer?>

### Why it is useful

<Why would you care?>

### Run it in the UI

<Metric key, filters, button / page section>

### Equivalent HTTP request

<Request body or query string>

### Result columns

<Column meanings>

### Notes

<Data prerequisites, limitations, interpretation traps>
```

When adding a new metric implementation, update:

- `IMetricExecutor` implementation and tests.
- DI registration in `src/Analyser/Program.cs`.
- `MetricCatalog` description.
- This document with at least one concrete example.

---

## 10. Known limitations and next improvements

- The current web UI is intentionally simple static HTML/JavaScript.
- Metric outputs are tabular, not charted.
- Square indices are numeric in the current knight metric output.
- Long-running analytics backfill is CLI-based and does not yet have the ETL progress bar.
- Metadata columns are only populated for games inserted after `PgnGameHeaderMapper` was wired in,
  unless a future metadata backfill is implemented.

