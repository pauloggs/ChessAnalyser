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

Discovery returns each metric key, a short description, and parameter hints for the most relevant
query fields. The web UI shows those hints under **Refresh metric list**.

Current metric keys:

- `AverageMaterialByYearAndColour`
- `KnightMoveDestinationFrequency`
- `GameCountByEco`
- `GameCountByYear`
- `GameCountByResult`
- `GameCountByPlayer`
- `PlayerResultSummary`
- `AverageMaterialByPlayerAtMove`
- `AverageCastlingPly`
- `AverageMaterialVolatility`
- `BishopPairFrequency`
- `MinorPieceComposition`

**Planned:** further playing-style metrics — see [STYLE_METRICS.md](./STYLE_METRICS.md) and
[PLAN.md §12.6](./PLAN.md) Phase 3 onward.

Metrics support the shared `AnalyticsQuery` filter shape where the filter is meaningful for that
metric:

```json
{
  "minGameYear": 1900,
  "maxGameYear": 1950,
  "playerSurname": null,
  "playerForenames": null,
  "playerColour": "Any",
  "eco": null,
  "playerASurname": "Kasparov",
  "playerAForenames": "Garry",
  "playerBSurname": null,
  "playerBForenames": null,
  "moveNumber": 1,
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
  filtering player to `Kasparov, Garry` with colour `White` compares Kasparov's White-side material
  with the Black-side material of his opponents in those same games. It does **not** compare Kasparov
  against an all-player baseline or against another player independently.
- Use the planned player-comparison metric (PLAN §12.5) for player-vs-player or player-vs-baseline
  material questions.

---

## 5. Example analysis: average material by player at move

### Question

How does one player's average material at a full move compare with another player, or with the
all-player baseline?

### Why it is useful

This is the player-comparison material metric. Unlike `AverageMaterialByYearAndColour`, it compares
player appearances independently, so Player A does not need to have played Player B.

### Run it in the UI

In **Analytics metrics**, select metric key `AverageMaterialByPlayerAtMove`. The form switches to
player-comparison fields.

Set:

- Player A (required)
- Player B (optional; leave as **All players** for the baseline)
- Colour: `Any`, `White`, or `Black`
- `moveNumber`: full move number (default `1`)
- Optional: `minGameYear`, `maxGameYear`, `eco`

Click **Run metric**.

### Equivalent HTTP request

```http
POST /api/analytics/metrics/execute
Content-Type: application/json
```

```json
{
  "metricKey": "AverageMaterialByPlayerAtMove",
  "query": {
    "playerASurname": "Kasparov",
    "playerAForenames": "Garry",
    "playerBSurname": "Karpov",
    "playerBForenames": "Anatoly",
    "playerColour": "Any",
    "moveNumber": 5,
    "minGameYear": 1980,
    "maxGameYear": 1990
  }
}
```

### Result columns

- `Series` (`PlayerA`, `PlayerB`, or `AllPlayers`)
- `Player`
- `Colour`
- `MoveNumber`
- `PlyIndex`
- `AvgMaterial`
- `PositionCount`

### Notes

- User-facing `moveNumber` maps to `PlyIndex = (moveNumber * 2) - 1`, i.e. after Black's move for
  that full move number.
- `playerColour = Any` includes both White and Black appearances for the selected player(s).
- If Player B is omitted, the comparison row is `AllPlayers`.
- If Player A does not match any rows, the metric returns an empty result.

---

## 6. Example analysis: knight destination frequency

### Question

Which destination squares do knights move to most often, optionally filtered by year, ECO, or
player?

### Why it is useful

This is a compact move-frequency analysis over the derived `GameMove` fact table. It is useful for
checking whether move derivation is working and for exploring opening or player tendencies.

### Run it in the UI

In **Analytics metrics**:

- Metric key: `KnightMoveDestinationFrequency`
- Optional: set `minGameYear`, `maxGameYear`, `eco`, or choose a player plus colour

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

## 7. Example analysis: game count by ECO

### Question

Which ECO codes occur most often in the loaded game set?

### Why it is useful

This is a fast corpus-shape metric. It helps you see which openings dominate the data and is easy
to verify against `dbo.Game`.

### Run it in the UI

In **Analytics metrics**:

- Metric key: `GameCountByEco`
- Optional: set `minGameYear`, `maxGameYear`, `eco`, or choose a player plus colour

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

## 8. Example analysis: game count by year

### Question

How are the loaded games distributed over calendar years?

### Why it is useful

This is a fast corpus-shape metric for checking whether the loaded PGNs cover the expected years
and whether `GameYear` parsing looks sensible before running deeper year-based analyses.

### Run it in the UI

In **Analytics metrics**:

- Metric key: `GameCountByYear`
- Optional: set `minGameYear`, `maxGameYear`, `eco`, or choose a player plus colour

Click **Run metric**.

### Equivalent HTTP request

```http
POST /api/analytics/metrics/execute
Content-Type: application/json
```

```json
{
  "metricKey": "GameCountByYear",
  "query": {
    "minGameYear": 1900,
    "maxGameYear": 1950
  }
}
```

### Result columns

- `GameYear`
- `GameCount`

### Notes

- Games with `GameYear = NULL` are excluded.
- Supplying `eco` narrows the year distribution to games with that exact ECO code.

---

## 9. Example analysis: game count by result

### Question

How often do games end in White wins, Black wins, draws, or unknown results?

### Why it is useful

This is a quick sanity check over the PGN result parsing and a useful corpus-shape metric for
understanding whether the loaded data is balanced by outcome.

### Run it in the UI

In **Analytics metrics**:

- Metric key: `GameCountByResult`
- Optional: set `minGameYear`, `maxGameYear`, `eco`, or choose a player plus colour

Click **Run metric**.

### Equivalent HTTP request

```http
POST /api/analytics/metrics/execute
Content-Type: application/json
```

```json
{
  "metricKey": "GameCountByResult",
  "query": {
    "minGameYear": 1900,
    "maxGameYear": 1950
  }
}
```

### Result columns

- `Result`
- `GameCount`

### Notes

- Stored winner codes are normalized to `White`, `Black`, `Draw`, or `Unknown`.
- `Unknown` includes games where the result was missing, not parsed, or otherwise not one of the
  known PGN result values.

---

## 10. Example analysis: game count by player

### Question

Which players appear most often in the loaded games, and how often did they play White or Black?

### Why it is useful

This is a quick participation metric over the resolved `Player` table. It helps identify dominant
players in the corpus and checks whether player resolution is producing sensible counts.

### Run it in the UI

In **Analytics metrics**:

- Metric key: `GameCountByPlayer`
- Optional: set `minGameYear`, `maxGameYear`, `eco`, or choose a player plus colour

Click **Run metric**.

### Equivalent HTTP request

```http
POST /api/analytics/metrics/execute
Content-Type: application/json
```

```json
{
  "metricKey": "GameCountByPlayer",
  "query": {
    "minGameYear": 1900,
    "maxGameYear": 1950
  }
}
```

### Result columns

- `Player`
- `WhiteGameCount`
- `BlackGameCount`
- `TotalGameCount`

### Notes

- Player filters narrow the game set before counting appearances.
- Only resolved players are included; games without a resolved player ID for a side do not
  contribute an appearance for that side.

---

## 11. Example analysis: player result summary

### Question

Which players have the most wins, losses, draws, and score in the loaded game set?

### Why it is useful

This summarizes results from each player's own perspective, regardless of whether they played White
or Black. It is useful for quick leaderboard-style checks and for spotting unusual result parsing.

### Run it in the UI

In **Analytics metrics**:

- Metric key: `PlayerResultSummary`
- Optional: set `minGameYear`, `maxGameYear`, `eco`, or choose a player plus colour

Click **Run metric**.

### Equivalent HTTP request

```http
POST /api/analytics/metrics/execute
Content-Type: application/json
```

```json
{
  "metricKey": "PlayerResultSummary",
  "query": {
    "minGameYear": 1900,
    "maxGameYear": 1950
  }
}
```

### Result columns

- `Player`
- `WinCount`
- `LossCount`
- `DrawCount`
- `UnknownCount`
- `TotalGameCount`
- `Score`

### Notes

- Wins and losses are calculated from the player's perspective.
- `Score` is `wins + (draws / 2)`.
- Player filters narrow the game set before summarizing results.

---

## 12. Example analysis: browse the game population

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

## 13. Useful SQL checks

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

## 14. Example analysis: average castling ply (playing style)

### Question

At what half-move does a player typically castle for the first time?

### Why it is useful

Early castling often indicates a pragmatic, king-safety-first approach; late or absent castling can
signal riskier structures. Compare players with the same year and colour filters.

### Run it in the UI

In **Analytics metrics**:

- Metric key: `AverageCastlingPly`
- **Required:** choose a player (surname; forenames recommended)
- Optional: `playerColour`, `minGameYear`, `maxGameYear`, `eco`

Click **Run metric**.

### Equivalent HTTP request

```http
POST /api/analytics/metrics/execute
Content-Type: application/json
```

```json
{
  "metricKey": "AverageCastlingPly",
  "query": {
    "playerSurname": "Petrosian",
    "playerForenames": "Tigran",
    "playerColour": "Any",
    "minGameYear": 1950,
    "maxGameYear": 1970
  }
}
```

### Result columns

- `Player` — filtered player name
- `GamesWithCastling` — games where the player castled at least once
- `AverageCastlingPly` — mean ply of first castle (games without castling excluded from the average)

### Notes

- Requires `dbo.GameMove` with castling flags populated.
- `playerSurname` is required; the API returns `400` if omitted.
- Pair with `UncastledKingRate` (planned Phase 5) for a fuller king-safety picture.

---

## 15. Example analysis: average material volatility (playing style)

### Question

How much does the material balance swing during a player's games?

### Why it is useful

High volatility suggests dynamic, imbalanced fighting; low volatility suggests stable positional
play. Useful for comparing Tal-like complexity against Karpov-like stability.

### Run it in the UI

In **Analytics metrics**:

- Metric key: `AverageMaterialVolatility`
- **Required:** choose a player
- Optional: `playerColour`, `minGameYear`, `maxGameYear`, `eco`
- Optional ply window: pass `minPlyIndex` / `maxPlyIndex` in the HTTP body (e.g. middlegame 15–40)

Click **Run metric**.

### Equivalent HTTP request

```http
POST /api/analytics/metrics/execute
Content-Type: application/json
```

```json
{
  "metricKey": "AverageMaterialVolatility",
  "query": {
    "playerSurname": "Tal",
    "playerForenames": "Mikhail",
    "playerColour": "Any",
    "minPlyIndex": 15,
    "maxPlyIndex": 40
  }
}
```

### Result columns

- `Player` — filtered player name
- `GameCount` — games with at least two position summaries in the ply window (needed for std dev)
- `AverageMaterialVolatility` — mean per-game sample standard deviation of signed material balance
  from the player's perspective

### Notes

- Balance is `WhiteMaterial - BlackMaterial` when the player had White, reversed when Black.
- Omit `minPlyIndex` / `maxPlyIndex` to include all plies (including ply `-1`).
- Requires `dbo.GamePositionSummary` rows.

---

## 16. Example analysis: bishop pair frequency (playing style)

### Question

How often does a player retain both bishops during the middlegame?

### Run it in the UI

- Metric key: `BishopPairFrequency`
- **Required:** player surname (and forenames recommended)
- Optional: `playerColour`, year/ECO filters
- Optional: `minPlyIndex` / `maxPlyIndex` (default **15–30** when both omitted)

### Equivalent HTTP request

```json
{
  "metricKey": "BishopPairFrequency",
  "query": {
    "playerSurname": "Karpov",
    "playerForenames": "Anatoly",
    "playerColour": "Any"
  }
}
```

### Result columns

- `Player`, `GameCount`, `AverageBishopPairFrequency` (0–1), `MinPlyIndex`, `MaxPlyIndex`

---

## 17. Example analysis: minor piece composition (playing style)

### Question

Does the player tend toward bishops or knights on the board in the middlegame?

### Run it in the UI

- Metric key: `MinorPieceComposition`
- **Required:** player surname
- Optional ply window (default **15–30**)

### Equivalent HTTP request

```json
{
  "metricKey": "MinorPieceComposition",
  "query": {
    "playerSurname": "Tal",
    "playerForenames": "Mikhail",
    "minPlyIndex": 15,
    "maxPlyIndex": 30
  }
}
```

### Result columns

- `Player`, `GameCount`, `AverageMinorPieceDelta` (positive = bishop-oriented), `MinPlyIndex`, `MaxPlyIndex`

---

## 18. Playing-style metrics (overview)

| Resource | Contents |
|----------|----------|
| [STYLE_METRICS.md](./STYLE_METRICS.md) | Literature review, style dimensions, profile combinations, caveats |
| [PLAN.md §12.6](./PLAN.md) | Ordered implementation backlog (Phases 3–9) |

---

## 19. Adding a new example analysis to this document

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

## 11. Known limitations and next improvements

- The current web UI is intentionally simple static HTML/JavaScript.
- Metric outputs are tabular, not charted.
- Square indices are numeric in the current knight metric output.
- Long-running analytics backfill is CLI-based and does not yet have the ETL progress bar.
- Metadata columns are only populated for games inserted after `PgnGameHeaderMapper` was wired in,
  unless a future metadata backfill is implemented.

