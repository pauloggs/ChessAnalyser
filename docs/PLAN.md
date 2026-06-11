# ChessAnalyser — Board-position analytics (PLAN)

**Location:** **`docs/`** — alongside [DESIGN.md](./DESIGN.md) and [AGENT_CONTEXT.md](./AGENT_CONTEXT.md).  
**Document status:** Stage 2 (design guide) + **Stage 3 complete** (§11 checklist, 2026-05-10) + **Stage 4 §12 checklist complete** (metrics HTTP API + DESIGN F-9 / Q7 alignment). **Active implementation direction:** §12.7 corpus benchmarks for style metrics, then §12.6 Phase 3; HTTP auth **deferred** while the app stays local-only / undeployed.  
**Authority:** Implements [DESIGN.md](./DESIGN.md). Update this plan when scope or decisions change.

---

## 1. Traceability

| DESIGN reference | PLAN coverage |
|------------------|---------------|
| §3.4 Primary / secondary facts | §4.2, §5.2, §6 |
| §3.5 Explicit `Game` columns | §4.1, §5.1 |
| F-1 … F-10, NFR-* | §5–§9 |
| §8 Q3 (year omitted if unknown) | §5.1, §7.2 |
| §8.5 C# domain logic; SQL for access / trivial aggregates | §5.4, §8 |
| F-9 / Q7 (programmatic access) | §12 (Stage 4 — HTTP surface for existing registry) |
| Playing-style metrics (research) | [STYLE_METRICS.md](./STYLE_METRICS.md); implementation §12.6 |
| Corpus benchmarks (F-11) | [DESIGN.md §12](./DESIGN.md); implementation §12.7 |

---

## 2. Prerequisites

| Item | Detail |
|------|--------|
| Runtime | **.NET 8** (matches `Services.csproj`). |
| Database | SQL Server; existing **DbUp** pipeline in `src/Migrations`. |
| Solution layout | `Interfaces` (DTOs), `Repositories` (Dapper, `SqlStatements`), `Services` (ETL, parsing, board logic), `Analyser` (host/UI if any). |
| New NuGet | **None required** for v1 unless you introduce CSV export or benchmarking tools later. |

**Install / run (no new global tools):**

```bash
dotnet run --project src/Migrations/Migrations.csproj
```

Ensure `ConnectionStrings:ChessConnection` in `src/Migrations/appsettings.json` (or env override) points at the target database.

---

## 3. Current baseline (as-is)

- **`dbo.Game`:** `Id`, `Name`, `GameId`, `Winner`, `WhitePlayerId`, `BlackPlayerId` (player FKs from `005_AddPlayerRefsToGame.sql`). **Tags are not persisted** — they exist only on the in-memory `Game` DTO during parse.
- **`dbo.BoardPosition`:** `(GameId, PlyIndex)` PK; `PlyIndex = -1` initial, `0,1,2,…` after each half-move; twelve `BIGINT` bitboards + `EnPassantTargetFile`.
- **ETL:** `EtlService` → `PlayerResolver` → `BoardPositionService.SetBoardPositions` → `PersistenceService.InsertGames` → `ChessRepository.InsertGame` + `InsertBoardPositions`.
- **Chess logic:** `BitBoardManipulator`, `BoardPositionsHelper`, etc. — reuse for material popcount and move inference where possible.

---

## 4. Schema changes (migrations)

Add new numbered scripts under `src/Migrations/Scripts/` (DbUp runs in **filename order** — use `006_…`, `007_…`).

### 4.1 `006_AddGameAnalyticsColumns.sql` (idempotent `ALTER` / `IF NOT EXISTS`)

Add nullable or defaulted columns on `dbo.Game` for dimensions called out in DESIGN §3.5. Suggested minimum v1 set:

| Column | Type | Notes |
|--------|------|--------|
| `Event` | `NVARCHAR(500)` NULL | PGN `[Event]` |
| `Site` | `NVARCHAR(500)` NULL | `[Site]` |
| `DateTag` | `NVARCHAR(32)` NULL | Raw `[Date]` for audit |
| `GameYear` | `SMALLINT` NULL | Parsed year only when full `YYYY` is valid; **NULL = omit from year-based metrics** (DESIGN Q3) |
| `Eco` | `NVARCHAR(16)` NULL | `[ECO]` |

Optional v1 or follow-up: `Round`, `WhiteElo`, `BlackElo`, `TimeControl` — add if you want filters without a second migration wave.

**Indexes (nonclustered, filtered where useful):**

- `IX_Game_GameYear` on `(GameYear)` **WHERE GameYear IS NOT NULL** (optional but good for year slices).
- `IX_Game_WhitePlayerId`, `IX_Game_BlackPlayerId` (if not already beneficial via FK lookups — add if query plans show scans).
- `IX_Game_Eco` on `(Eco)` WHERE `Eco IS NOT NULL` (optional).

### 4.2 `007_CreateGameMoveTable.sql`

**Secondary fact:** one row per half-move that advances the game from `PlyIndex - 1` → `PlyIndex` (for `PlyIndex >= 0`).

Suggested columns:

| Column | Type | Notes |
|--------|------|--------|
| `GameId` | `INT` NOT NULL | FK → `Game.Id` ON DELETE CASCADE |
| `PlyIndex` | `INT` NOT NULL | Same convention as `BoardPosition` |
| `MovingSide` | `CHAR(1)` NOT NULL | `'W'` / `'B'` — must match `PlyIndex % 2` with existing ply convention (see §7.1) |
| `FromSquare` | `TINYINT` NOT NULL | 0–63 (a1=0 … h8=63) — document mapping in code + tests |
| `ToSquare` | `TINYINT` NOT NULL | |
| `MovedPiece` | `CHAR(1)` NOT NULL | `P,N,B,R,Q,K` (piece type without colour) |
| `CapturedPiece` | `CHAR(1)` NULL | Same set if capture; NULL otherwise |
| `PromotionPiece` | `CHAR(1)` NULL | `N,B,R,Q` if promotion |
| `IsCastlingKingside` | `BIT` NOT NULL DEFAULT 0 | |
| `IsCastlingQueenside` | `BIT` NOT NULL DEFAULT 0 | |

**PK:** `(GameId, PlyIndex)`.  
**Index:** `IX_GameMove_ToSquare_MovedPiece` (or similar) for knight-to-e5 style queries.

### 4.3 `008_CreateGamePositionSummaryTable.sql` (rollup / denormalized per ply)

**Purpose:** avoid decoding 12 bitboards on every board-state metric query. Populated **only by C# batch** (DESIGN F-8, F-10).

Suggested columns (all `INT` or `SMALLINT` as appropriate):

- `GameId`, `PlyIndex` (PK with `GameId`), FK CASCADE to `Game`.
- `WhiteMaterial`, `BlackMaterial` — using the **named valuation policy** in code (DESIGN NFR-4).
- Per-type counts for White and Black: `WhitePawnCount`, … `BlackQueenCount` (twelve counts; kings usually 1).

**Optional v2:** compress into fewer columns or add generated columns in DB — **not** in v1 (keep DB dumb).

---

## 5. Application changes (by layer)

### 5.1 Interfaces (`Interfaces`)

1. Extend **`Game`** with properties matching new columns (`Event`, `Site`, `DateTag`, `GameYear`, `Eco`, …).
2. New DTOs (names indicative — adjust to your naming style):
   - **`GameMoveFact`** — maps to `GameMove` row.
   - **`GamePositionSummary`** — maps to `GamePositionSummary` row.
3. **Analytics query contracts** (internal v1):
   - `AnalyticsQuery` — filters (nullable year range, player ids, ECO, ply range, etc.).
   - `AnalyticsGroupBy` — list or flags for dimensions to group on.
   - `MetricId` or string key — identifies registered metric.
   - `AnalyticsTableResult` — column names + rows (e.g. `IReadOnlyList<object?>` or typed rows per metric).

### 5.2 Repositories (`Repositories`)

1. **`SqlStatements`:** extend `InsertGame` to include new columns; add `InsertGameMove`, bulk-friendly pattern (TVP or batched `INSERT` batches of N rows — choose one; Dapper multi-execute is fine for moderate ply counts).
2. **`ChessRepository` (or split `AnalyticsRepository`):**
   - `InsertGameMoveRows(int gameId, IReadOnlyList<GameMoveFact> rows)` inside same transaction as positions **or** immediately after `InsertBoardPositions` in a second transaction (prefer **one transaction** per game for atomicity: game + positions + moves + summaries).
   - `InsertGamePositionSummaries(...)`.
   - Read APIs for metrics: e.g. `GetPositionSummariesForGamesAsync` with filters, **parameterized** SQL only (DESIGN NFR-6).
3. **Backfill:** `GetGameIdsMissingMovesAsync` / `GetBoardPositionsForGameOrderedAsync` — stream `BoardPosition` rows for derivation when analytics tables are empty for a game.

### 5.3 Services (`Services`)

#### 5.3.1 PGN tag extraction

- Small helper **`PgnGameHeaderMapper`** (or extend existing parser output path): when `Game.Tags` is populated, set `Game.Event`, `Game.GameYear`, etc.
- **Year parsing:** if `[Date]` matches `^\d{4}\.\d{2}\.\d{2}``, parse year = first segment; else leave `GameYear` null. Do **not** guess from `Event` (DESIGN).

#### 5.3.2 Move derivation (pure C#)

- **`IGameMoveDeriver`** / **`GameMoveDeriver`:** input `(BoardPosition previous, BoardPosition current, int plyIndex)` → `GameMoveFact`.
- Algorithm sketch:
  1. XOR each piece-type bitboard between previous and current to find squares where occupancy changed.
  2. Classify: normal move, capture, promotion, castling (king moves two files) using existing square/piece reads (`BitBoardManipulator.ReadSquare` / popcount).
  3. Assert **exactly one** friendly piece origin and one legal move shape per ply; on failure log + skip row or record parse-quality flag (decide: **skip move row** and optionally extend `GameParseError` — document in metric specs).
- **Unit tests** with known PGN-derived pairs from test fixtures (NFR-9).

#### 5.3.3 Position summary (pure C#)

- **`IGamePositionSummaryFactory`:** from `BoardPosition` + valuation policy interface **`IPieceValues`** (default classical), produce `GamePositionSummary`.

#### 5.3.4 Analytics execution framework

- **`IMetricExecutor`** — `string MetricKey`, `Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken ct)`.
- **`IMetricRegistry`** — register all executors (DI or explicit list in composition root).
- **Reference metrics (DESIGN §10):**
  1. **`AverageMaterialByYearAndColour`** — filter games with `GameYear NOT NULL`; group by `GameYear`, aggregate averages of `WhiteMaterial` / `BlackMaterial` from `GamePositionSummary` joined to `Game` at `PlyIndex` filter (e.g. fixed ply = 4).
  2. **`KnightMoveDestinationFrequency`** — from `GameMove` where `MovedPiece = 'N'`; `GROUP BY` `ToSquare` (+ optional filters: year, player, ECO); counts in **C#** after parameterized read, or `COUNT(*)` in SQL grouped by narrow columns only (DESIGN §8.5).

#### 5.3.5 Orchestration

- **`IAnalyticsMaterializationService`** (name flexible):
  - `MaterializeAfterGamePersistedAsync(Game game, int databaseGameId)` — uses **in-memory** `game.BoardPositions` ordered by ply to build move facts + summaries, then persists. Called from **`PersistenceService.InsertGames`** after `InsertGame` + `InsertBoardPositions` **or** from `ChessRepository` if you keep all DB writes in one place.
- **`IAnalyticsBackfillService`** — for games already in DB: scan games without `GameMove` rows, load `BoardPosition` from DB, hydrate DTOs, derive, insert (batch by game).

### 5.4 Dependency injection

- Register new services in **`Analyser`** (or whichever project builds the host) — locate existing `Program.cs` / `Startup` pattern and mirror.

---

## 6. ETL integration order

Per game (unchanged high-level flow until persistence):

1. Parse PGN → `Game` with `Tags`.
2. Map headers → new `Game` fields (`GameYear`, `Eco`, …).
3. Resolve players.
4. `SetBoardPositions`.
5. `InsertGame` (extended SQL).
6. `InsertBoardPositions`.
7. **NEW:** `MaterializeAfterGamePersistedAsync` (moves + summaries) — same transaction as 5–6 if feasible; otherwise sequential with clear rollback policy documented in code.

**Idempotency:** If the same `GameId` is skipped as “already processed,” backfill job handles analytics gaps separately.

---

## 7. Conventions to lock in code + tests

### 7.1 Ply and side

- **`PlyIndex -1`:** initial position. **`0`:** after White’s first half-move (matches existing `Game` / `BoardPositionService` comments).
- **`MovingSide` for ply `p >= 0`:** White if `p % 2 == 0`, Black if `p % 2 == 1`. **Verify** against a few canonical games in tests (NFR-5).

### 7.2 Year-based metrics

- Any executor that groups or filters by year must use **`Game.GameYear IS NOT NULL`** in its **repository filter** (or equivalent in-memory filter after read).

### 7.3 Valuation policy

- Implement **`IPieceValues`** with a default profile; pass into summary factory and document in metric specs (NFR-4).

---

## 8. SQL vs C# boundary (operational rule)

- **Allowed in SQL:** `SELECT`/`JOIN`/`WHERE`/`ORDER BY` on indexed columns; optional `COUNT(*)` / `SUM`/`AVG` over **scalar columns** of `GamePositionSummary` / `GameMove` / `Game`.
- **Forbidden in SQL:** bitboard interpretation, move reconstruction, material formulas.

---

## 9. Testing plan

| Layer | Tests |
|-------|--------|
| `GameMoveDeriver` | Golden cases: e2-e4, capture, promotion, O-O, O-O-O, en passant (if test data available). |
| `GamePositionSummaryFactory` | Known starting position counts and material totals. |
| `PgnGameHeaderMapper` / year | `????.??.??` → null year; `1934.01.01` → 1934. |
| Metric executors | Mock repository returning small fixed datasets; assert aggregation. |
| Repository integration (optional) | LocalDB / test DB: one game round-trip including moves + summaries. |

---

## 10. Documentation updates (same PR wave as implementation)

- **`src/Migrations/README.md`:** add rows for scripts `006`–`008` and describe `GameMove` / `GamePositionSummary`.
- **`docs/DESIGN.md`:** only if decisions drift; otherwise no change.

---

## 11. Implementation task checklist (ordered)

Use this as the working backlog for stage 3 (IMPLEMENT).

1. [x] Add `006_AddGameAnalyticsColumns.sql`; run DbUp on dev DB.
2. [x] Add `007_CreateGameMoveTable.sql`; run DbUp.
3. [x] Add `008_CreateGamePositionSummaryTable.sql`; run DbUp.
4. [x] Extend `Game` DTO + `InsertGame` SQL + `ChessRepository.InsertGame` parameters.
5. [x] Implement header/year/ECO mapping from `Tags` during parse or immediately before insert (single place).
6. [x] Implement `IPieceValues` + `GamePositionSummaryFactory` + tests.
7. [x] Implement `GameMoveDeriver` + tests.
8. [x] Add repository insert/read methods + `SqlStatements` entries + tests (mock or integration).
9. [x] Implement `IAnalyticsMaterializationService`; wire into `PersistenceService` / ETL path after board insert.
10. [x] Implement `IMetricRegistry` + two reference metric executors + tests.
11. [x] Implement optional `IAnalyticsBackfillService` + CLI or admin entry point (could be a simple `dotnet run` mode on `Analyser` or a one-off console flag — decide at implement time).
12. [x] Performance smoke: note approximate rows/sec for materialization on your machine (DESIGN NFR-3).
13. [x] Update `Migrations/README.md`.

### 11.1 Stage 3 closure

Items **1–13** are **complete** in source for the board-position analytics groundwork: migrations through `010`, ETL materialization, **`IMetricRegistry`** and reference executors, backfill and perf CLIs on `Analyser`, and `Migrations/README.md` / [ANALYTICS_MATERIALIZATION_PERF.md](./ANALYTICS_MATERIALIZATION_PERF.md). **§11 is frozen** as the historical implementation checklist.

**Active backlog:** use **§12** below (including **§12.4** for the current suggested slice). For session handoff text, prefer [AGENT_CONTEXT.md](./AGENT_CONTEXT.md).

---

## 12. Stage 4 — Analytics metrics HTTP API (`Analyser`)

**Goal:** Expose the existing **`IMetricRegistry`** (and **`AnalyticsQuery`** filters) over HTTP so external clients, UI, and integration tests can run registered metrics without referencing `Services` assemblies directly.

**Non-goals for the first API slice:** full generic “analytics query language”, ad-hoc SQL from clients, or bitboard exposure over HTTP (keep tabular scalar results only).

### 12.1 API shape (v1 proposal — adjust names to match repo conventions)

| Concern | Decision |
|---------|----------|
| Execute | **`POST /api/analytics/metrics/execute`** (or `…/run`) with body `{ "metricKey": "…", "query": { … } }` mapping to **`AnalyticsQuery`** (`MinGameYear`, `MaxGameYear`, independent `PlayerSurname` / `PlayerForenames` + `PlayerColour`, `Eco`, `SummaryPlyIndex`, and metric-specific player comparison fields). |
| Discovery | **`GET /api/analytics/metrics`** returning **`IMetricRegistry.MetricKeys`** (and optional short descriptions from a static map or XML comments). |
| Response | JSON projection of **`AnalyticsTableResult`**: `columnNames` + `rows` (array of arrays, or array of objects keyed by column — pick one and document). |
| Errors | **404** or **400** for unknown **`metricKey`**; **400** for malformed filters. |
| Auth | **Deferred** while the host is **solo, local-only, and not deployed** for shared or internet access — no dedicated auth work in the current phase. If the API is later exposed on a network or to other users, add auth (e.g. API key or reverse proxy) + rate limits and update §13. Until then, optional hygiene: bind the web host to **localhost** only in run configuration. |
| OpenAPI | Reuse Swashbuckle; ensure DTOs use public properties for usable schemas. |

### 12.2 Implementation checklist (ordered)

1. [x] Add API request/response DTOs (Analyser or small shared project — avoid pulling web types into `Interfaces` unless you already do).
2. [x] Add controller (e.g. **`AnalyticsMetricsController`**) calling **`IMetricRegistry.ExecuteAsync`** and mapping results.
3. [x] Add discovery endpoint listing keys from **`IMetricRegistry`**.
4. [x] Add **`ControllerTests`** (or existing test host pattern) with **`IMetricRegistry`** mocked — assert status codes and JSON shape for at least one known key.
5. [x] Update **DESIGN.md** F-9 / Q7 to reflect “HTTP available for metrics” (or equivalent wording) in the **same** or **immediately following** PR.

### 12.3 Testing additions

| Layer | Tests |
|-------|-------|
| API | Happy path + unknown metric key + optional filter passthrough (mock registry). |

### 12.4 Recommended next implementation slice (post–§12)

**Decision (2026):** The maintainer uses the application **only on a local machine** with **no current plan** to deploy it or grant controlled network access. Under that threat model, **HTTP authentication and rate-limiting for `AnalyticsMetricsController` are not the next priority.**

**Suggested next work (PR-sized, in order of value):**

1. [x] **Unified local web UI (`wwwroot`)** — primary surface for PGN load, metrics, and paged games.
2. [x] **Extend the metrics surface** — corpus and player summary metrics (`GameCountByEco`,
   `AverageMaterialByPlayerAtMove`, `GameCountByYear`, `GameCountByResult`, `GameCountByPlayer`,
   `PlayerResultSummary`).
3. [x] **Improve discovery** — descriptions and parameter hints on `GET /api/analytics/metrics`.
4. [x] **Playing-style metrics Phases 1–2** — `AverageCastlingPly`, `AverageMaterialVolatility`,
   `BishopPairFrequency`, `MinorPieceComposition` (see §12.6).
5. **Corpus benchmarks** — **[§12.7](./PLAN.md)** before §12.6 Phase 3; design [DESIGN.md §12](./DESIGN.md).
6. **Playing-style metrics Phase 3+** — `CaptureRate`, `QueenTradeRate`, … per §12.6 after benchmarks v1.
7. **Tests** — per-metric executor tests with mocked **`IChessRepository`** (and API smoke tests for
   new keys), following §12.3.

**Optional:** Record a fresh materialization throughput line in [ANALYTICS_MATERIALIZATION_PERF.md](./ANALYTICS_MATERIALIZATION_PERF.md) when you change hot paths in the deriver or summary factory.

**When auth becomes relevant:** If you deploy the host or open it beyond localhost, treat **auth + rate limits** (or documented reverse-proxy-only access) as a **blocking** task before wider use; update §12.1, §13, and [AGENT_CONTEXT.md](./AGENT_CONTEXT.md).

---

### 12.5 Player material comparison metrics (planned PR sequence)

**Decision (2026):** The existing **`AverageMaterialByYearAndColour`** metric is useful as a
general corpus/year trend, but it is **not** a clear player-comparison metric. If a White player is
selected and Black is left as “any”, the result compares that selected player’s White material
against the Black-side material from the same games. That can be misleading because it does **not**
compare two players independently, nor does it compare a player against a corpus baseline.

Implement player material comparison as a separate sequence of PRs:

1. [x] **Clarify the existing metric (docs/catalog only).**
   - Keep **`AverageMaterialByYearAndColour`** as a corpus-oriented metric.
   - Update metric catalog / examples so it is described as “average side material by year”, not a
     player comparison tool.
   - Avoid implying that filtering by one player creates a fair independent comparison.
2. [x] **Add a new player-comparison metric.**
   - Indicative key: **`AverageMaterialByPlayerAtMove`** (final name can change during
     implementation).
   - Inputs:
     - Player A (`Surname`, `Forenames`).
     - Optional Player B (`Surname`, `Forenames`).
     - Optional colour filter: **White**, **Black**, or **Any**.
     - Move number (user-facing), not raw `PlyIndex`.
     - Optional year / ECO filters, following existing `AnalyticsQuery` style.
   - Supported comparisons:
     - Selected White player average material vs selected Black player average material.
     - Selected player average material vs **all players** baseline.
     - Player A vs Player B regardless of colour, when colour is **Any**.
   - Keep the DB model ply-based internally; translate user-facing move number to the appropriate
     `PlyIndex` in the metric or repository layer.
3. [x] **Add metric-specific UI affordances.**
   - The generic metrics form should not try to make every filter meaningful for every metric.
   - When `AverageMaterialByPlayerAtMove` is selected, show player-comparison fields, colour mode,
     and move number wording that explains the internal ply convention.
   - Keep raw metric execution available for development / Swagger.

**Move vs ply rule for this planned metric:**

- The public UI/API should ask for **move number**, because users think in moves, not raw plies.
- Internally, `GamePositionSummary` remains keyed by `PlyIndex`.
- Current convention:
  - `PlyIndex = -1`: initial position.
  - `PlyIndex = 0`: after White move 1.
  - `PlyIndex = 1`: after Black move 1.
- Implementation must make the chosen interpretation explicit. Preferred initial option:
  **“after full move N”** maps to `PlyIndex = (N * 2) - 1` (after Black’s Nth move). If later
  needed, add “after White move N” as a separate selector rather than overloading one number.

**Testing expectations:**

- Unit tests for move-number-to-ply mapping, including move 1.
- Executor tests covering:
  - Player A vs all-player baseline.
  - Player A vs Player B.
  - Colour = White, Black, Any.
  - Empty result when the player name does not match.
- API/controller or UI smoke tests if metric-specific UI logic becomes non-trivial.

---

### 12.6 Playing-style metrics (planned PR sequence)

**Decision (2026):** Extend the metrics catalog toward **playing-style fingerprints** — behavioural
patterns derived from `GameMove`, `GamePositionSummary`, and `Game` **without engine evaluation**.
Research background, literature references, profile combinations, and interpretation caveats are
documented in [STYLE_METRICS.md](./STYLE_METRICS.md).

**Shared conventions for §12.6 metrics:**

- Follow existing **`IMetricExecutor`** + parameterized **`IChessRepository`** read pattern (§5.3.4,
  §8).
- Player filters use **`AnalyticsQuery.PlayerSurname`**, **`PlayerForenames`**, and
  **`PlayerColour`** (`Any` / `White` / `Black`) — independent identity and colour (see
  `fix/independent-metric-player-filter`).
- Year / ECO filters reuse existing `AnalyticsQuery` fields where applicable.
- Register each executor in **`Program.cs`**; add **`MetricCatalog`** description + parameter hints;
  add executor tests with mocked repository; add an [EXAMPLE_ANALYSES.md](./EXAMPLE_ANALYSES.md)
  entry when the metric ships.
- **Player filter required** for per-player style metrics unless the metric is explicitly corpus-wide
  (none in the first phases).
- Prefer **one PR per metric** (or one PR per small phase) to keep review small.

**Optional query extensions** (add to `AnalyticsQuery` when the first metric in a phase needs them):

| Field | Type | Purpose |
|-------|------|---------|
| `minPlyIndex` / `maxPlyIndex` | `int?` | Restrict move or position reads to a ply window (e.g. middlegame 15–40). |
| `queenTradeMaxPly` | `int?` | For `QueenTradeRate` — queens exchanged on or before this ply count as “early trade”. |
| `shortDrawMaxPly` | `int?` | For `ShortDrawRate` — draws at or below this length count as “short”. |
| `ecoTopN` | `int?` | For `EcoConcentration` — N largest ECO families (default 3). |

Document defaults in `MetricCatalog.GetParameterHints` when added.

---

#### Phase 1 — castling tempo and material volatility (implement first)

1. [x] **`AverageCastlingPly`**
   - **Question:** At what half-move does a player typically castle?
   - **Style signal:** Early castling ≈ pragmatic / king-safety-first; late or absent ≈ riskier or
     more aggressive structures (ChessBase **Risk**; thesis game-structure features).
   - **Data:** `GameMove` where `IsCastlingKingside = 1 OR IsCastlingQueenside = 1`, joined to
     `Game` + `Player` for the filtered side.
   - **Per game:** `MIN(PlyIndex)` of the player's first castling move (one value per player per
     game).
   - **Aggregate:** `AVG(castling_ply)` over games where the player castled at least once.
   - **Result columns:** `PlayerSurname`, `PlayerForenames`, `GamesWithCastling`, `AverageCastlingPly`
     (nullable decimal; round to 1 decimal in presentation layer if desired).
   - **Exclude** games where the player never castled from the average (report
     `GamesWithCastling` separately; pair with Phase 5 `UncastledKingRate`).
   - **Filters:** `playerSurname`, `playerForenames`, `playerColour`, `minGameYear`, `maxGameYear`,
     `eco`.
   - **Tests:** mock repository — player castles once at ply 6 and once at ply 10 → avg 8; games
     without castling excluded from average but counted correctly.

2. [x] **`MaterialVolatility`** (metric key: `AverageMaterialVolatility`)
   - **Question:** How much does the material balance swing during a player's games?
   - **Style signal:** High volatility ≈ dynamic / imbalanced fighting; low ≈ stable positional grind
     (thesis **Mstd** / material-dynamics features; jk_182 imbalance axis).
   - **Data:** `GamePositionSummary` per ply, joined to `Game` for player side.
   - **Per game:** For each ply `p`, compute signed balance from the **player's perspective**:
     - White player: `WhiteMaterial - BlackMaterial`
     - Black player: `BlackMaterial - WhiteMaterial`
     - `playerColour = Any`: use side the filtered player had in that game (two rows per game if the
       same person played both colours — rare; normally filter colour or one row per game).
     - `STDDEV_SAMP(balance)` across all plies in the game (include ply `-1` through terminal ply,
       or document exclusion of ply `-1` — prefer **include all plies** for consistency).
   - **Aggregate:** `AVG(per_game_stddev)` across games.
   - **Result columns:** `PlayerSurname`, `PlayerForenames`, `GameCount`, `AverageMaterialVolatility`.
   - **Filters:** `playerSurname`, `playerForenames`, `playerColour`, `minGameYear`, `maxGameYear`,
     `eco`, optional `minPlyIndex` / `maxPlyIndex` to restrict which plies enter the std-dev (e.g.
     middlegame only).
   - **SQL note:** `STDDEV_SAMP` per game may be computed in SQL over grouped rows or in C# after a
     narrow read — choose per §8 (trivial scalar aggregate on summary columns is allowed).
   - **Tests:** flat balance → volatility 0; single spike → positive volatility; ply window respected.

---

#### Phase 2 — piece philosophy

3. [x] **`BishopPairFrequency`**
   - **Question:** How often does a player retain both bishops in the middlegame?
   - **Data:** `GamePositionSummary` — count plies in window where `WhiteBishopCount = 2` (or
     `BlackBishopCount = 2`) for the player's side.
   - **Per game:** `plies_with_bishop_pair / plies_in_window` (ratio 0–1).
   - **Aggregate:** `AVG(ratio)` across games.
   - **Result columns:** `PlayerSurname`, `PlayerForenames`, `GameCount`, `AverageBishopPairFrequency`.
   - **Default ply window:** plies 15–30 (override via `minPlyIndex` / `maxPlyIndex`).
   - **Filters:** player + year + ECO + ply window.

4. [x] **`MinorPieceComposition`**
   - **Question:** Does the player tend toward bishops or knights on the board?
   - **Data:** `GamePositionSummary` in ply window.
   - **Per ply (player side):** `bishop_count - knight_count`; average across plies in window, then
     average across games.
   - **Result columns:** `PlayerSurname`, `PlayerForenames`, `GameCount`, `AverageMinorPieceDelta`
     (positive = bishop-oriented).
   - **Default ply window:** plies 15–30.

---

#### Phase 3 — exchange temperament

5. [x] **`CaptureRate`**
   - **Question:** What fraction of the player's moves are captures?
   - **Data:** `GameMove` where `MovingSide` matches the player's colour in that game.
   - **Per game:** `COUNT(captures) / COUNT(moves)` where capture ⇔ `CapturedPiece IS NOT NULL`.
   - **Aggregate:** `AVG(per_game_rate)`.
   - **Result columns:** `PlayerSurname`, `PlayerForenames`, `GameCount`, `AverageCaptureRate`.
   - **Optional:** `minPlyIndex` / `maxPlyIndex` on move ply.

6. [x] **`QueenTradeRate`**
   - **Question:** How often are queens exchanged early?
   - **Data:** `GamePositionSummary` or `GameMove` — detect first ply where both
     `WhiteQueenCount` and `BlackQueenCount` are not both 1 (or either queen count drops to 0).
   - **Per game:** `1` if queen trade occurred on or before `queenTradeMaxPly` (default **40**),
     else `0`.
   - **Aggregate:** `AVG` → proportion of games with early queen trade.
   - **Result columns:** `PlayerSurname`, `PlayerForenames`, `GameCount`, `QueenTradeRate`,
     `QueenTradeMaxPly` (echo parameter used).

---

#### Phase 4 — spatial aggression

7. [x] **`CentreMoveRate`**
   - **Question:** How often does a player play to central squares?
   - **Data:** `GameMove` — `ToSquare` in centre set **{d4, d5, e4, e5}** (square indices 27, 28,
     35, 36 with a1 = 0).
   - **Per game:** centre moves / total moves by player.
   - **Aggregate:** `AVG(per_game_rate)`.
   - **Result columns:** `PlayerSurname`, `PlayerForenames`, `GameCount`, `AverageCentreMoveRate`.
   - **Optional v2:** extend centre ring to c3–f6 (document if added).

8. [x] **`ForwardMoveRate`**
   - **Question:** How often does a player advance into the opponent's half?
   - **Data:** `GameMove` — `ToSquare` rank index (0–7) compared to moving side (White: rank ≥ 4;
     Black: rank ≤ 3).
   - **Per game:** forward moves / total moves.
   - **Aggregate:** `AVG(per_game_rate)`.
   - **Result columns:** `PlayerSurname`, `PlayerForenames`, `GameCount`, `AverageForwardMoveRate`.

---

#### Phase 5 — king safety extensions

9. [x] **`CastlingSidePreference`**
   - **Data:** `GameMove` castling rows for the player.
   - **Per game:** first castle only — kingside vs queenside.
   - **Aggregate:** `KingsideRate`, `QueensideRate` (proportions among games with castling).
   - **Result columns:** `PlayerSurname`, `PlayerForenames`, `GamesWithCastling`, `KingsideRate`,
     `QueensideRate`.

10. [ ] **`OppositeSideCastlingRate`**
    - **Data:** first castling ply and side per colour per game from `GameMove`.
    - **Per game:** `1` if White and Black castled to different wings (kingside = K-side file,
      queenside = Q-side), else `0` (exclude games where either side never castled, or report
      separate `EligibleGameCount`).
    - **Aggregate:** proportion over eligible games.
    - **Result columns:** `EligibleGameCount`, `OppositeSideCastlingRate`.

11. [ ] **`UncastledKingRate`**
    - **Per game:** `1` if the filtered player never castled, else `0`.
    - **Aggregate:** proportion.
    - **Result columns:** `GameCount`, `UncastledKingRate`.

---

#### Phase 6 — queen timing

12. [ ] **`FirstQueenMovePly`**
    - **Data:** `GameMove` where `MovedPiece = 'Q'` for the player's side.
    - **Per game:** `MIN(PlyIndex)` of queen moves; normalize optionally as
      `first_queen_ply / max_ply` (document if normalization is included — v1 can report raw ply).
    - **Aggregate:** `AVG(first_queen_ply)` over games where the queen moved at least once.
    - **Result columns:** `PlayerSurname`, `PlayerForenames`, `GamesWithQueenMove`,
      `AverageFirstQueenMovePly`.

---

#### Phase 7 — game shape

13. [ ] **`AverageGameLength`**
    - **Data:** `MAX(PlyIndex)` from `GamePositionSummary` or move count per game.
    - **Aggregate:** `AVG(max_ply)` for games involving the filtered player.
    - **Result columns:** `PlayerSurname`, `PlayerForenames`, `GameCount`, `AverageGameLengthPly`.

14. [ ] **`ShortDrawRate`**
    - **Data:** `Game.Winner` draw detection + game length.
    - **Per game:** among draws only, `1` if `max_ply <= shortDrawMaxPly` (default **20**), else `0`.
    - **Aggregate:** proportion of draws that are short (ChessBase **Fighting Spirit** proxy).
    - **Result columns:** `DrawCount`, `ShortDrawRate`, `ShortDrawMaxPly`.

---

#### Phase 8 — repertoire shape

15. [ ] **`EcoDiversity`**
    - **Data:** `Game.Eco` for games involving the player.
    - **Aggregate:** `COUNT(DISTINCT Eco)` (exclude null ECO).
    - **Result columns:** `PlayerSurname`, `PlayerForenames`, `GameCount`, `EcoDiversity`.

16. [ ] **`EcoConcentration`**
    - **Data:** `Game.Eco` frequency table per player.
    - **Aggregate:** sum of game shares of the top `ecoTopN` ECO codes (default **3**).
    - **Result columns:** `PlayerSurname`, `PlayerForenames`, `GameCount`, `EcoConcentration`,
      `EcoTopN`.

---

#### Phase 9 — materialization extensions (defer until Phases 1–8 stable)

17. [ ] **`IsCheck`** on `GameMove` (derive at materialization from board diff) → enables future
    **CheckRate** metric.
18. [ ] **Game phase** label on `GamePositionSummary` (opening / middlegame / endgame by piece-count
    rules) → enables **PhaseDistribution** metric.
19. [ ] **`PlayerStyleProfile`** — composite metric returning a small vector of normalized scores
    (aggression, positional, simplifier, risk) from a subset of the above; optional PCA later.

**Engine-dependent (out of scope for §12.6):** ACPL, move accuracy, position sharpness/WDL,
sound sacrifice detection — see [STYLE_METRICS.md §4 Phase 9](./STYLE_METRICS.md) and §14 below.

**Validation:** After Phase 1, compare two players with contrasting reputations (e.g. Tal vs
Petrosian) on `AverageCastlingPly` and `MaterialVolatility` with the same filters; see
[STYLE_METRICS.md §7](./STYLE_METRICS.md).

**Benchmark dependency:** Phase 3+ metrics should use the shared benchmark enrichment from §12.7
when `includeCorpusBenchmark` is supported, rather than adding more raw-only scalars.

---

### 12.7 Corpus benchmarks for style metrics (implement before §12.6 Phase 3)

**Decision (2026):** Raw style scalars (e.g. Fischer `AverageMaterialVolatility ≈ 1.34`) are weakly
informative without a **corpus-relative** reference. Implement optional benchmark columns per
[DESIGN.md §12](./DESIGN.md) before adding Phase 3 style metrics.

**Goals:**

- Same metric definition and **non-identity** filters for subject and corpus.
- Subject excluded from corpus mean and percentile (leave-one-out).
- Opt-in via `includeCorpusBenchmark`; default **`false`** until callers migrate (document in
  `MetricCatalog` when style metrics flip default to `true`).

#### 12.7.1 Query extensions

Add to **`AnalyticsQuery`** (`Interfaces/Analytics/AnalyticsQuery.cs`):

| Field | Type | Default when null |
|-------|------|-----------------|
| `IncludeCorpusBenchmark` | `bool?` | `false` |
| `BenchmarkMinGames` | `int?` | `30` |

Expose in HTTP JSON as `includeCorpusBenchmark`, `benchmarkMinGames`. UI: optional checkbox on style
metrics section (follow-up after API).

#### 12.7.2 Shared components

1. [x] **`CorpusBenchmarkResult`** (or fields on existing row DTOs) — `CorpusAverage`,
   `DeltaFromCorpus`, `CorpusPercentile`, `CorpusEligiblePlayerCount` (all nullable when benchmark
   not requested or corpus too small).
2. [x] **`ICorpusBenchmarkCalculator`** (pure C#) — inputs: subject value, read-only list of
   `(playerId or name, perPlayerValue, gameCount)` for eligible corpus players → outputs benchmark
   fields. Unit-test percentile edge cases (ties, n=1, subject at min/max).
3. [x] **Repository pattern** — per metric, either:
   - **(Preferred)** one SQL statement returning subject row + corpus distribution via CTEs
     (`PerPlayerMetric` → `CorpusStats`), or
   - two reads: subject aggregate + all per-player aggregates (acceptable for v1 if SQL complexity
     is high).

Document chosen approach in code remarks per metric.

**Corpus eligibility rule:** player appears in ≥ `benchmarkMinGames` games in the filtered game set
(same rule as DESIGN §12.3).

**Percentile:** `PERCENT_RANK`-style over eligible per-player values (0–100 scale); document tie
handling in tests.

#### 12.7.3 Implementation sequence (PR-sized)

1. [x] **Infrastructure PR** — `AnalyticsQuery` fields, `ICorpusBenchmarkCalculator` + tests, row
   DTO optional benchmark fields.
2. [x] **`AverageMaterialVolatility` + benchmark** — proof of concept; extend executor columns when
   `includeCorpusBenchmark = true`; per-player repository SQL + calculator; update `MetricCatalog` and
   [EXAMPLE_ANALYSES.md](./EXAMPLE_ANALYSES.md).
3. [x] **`BishopPairFrequency` + benchmark** — reuse shared calculator / SQL pattern.
4. [x] **`MinorPieceComposition` + benchmark**
5. [x] **`AverageCastlingPly` + benchmark**
6. [x] **Docs pass** — [STYLE_METRICS.md](./STYLE_METRICS.md) §8, `AGENT_CONTEXT.md`, example
   showing Fischer row with corpus columns.

**Result columns when benchmark enabled** (append to existing subject row):

- `CorpusAverage`
- `DeltaFromCorpus`
- `CorpusPercentile`
- `CorpusEligiblePlayerCount`

#### 12.7.4 Testing

| Layer | Tests |
|-------|--------|
| `CorpusBenchmarkCalculator` | Empty corpus; single player; ties; subject excluded from mean; percentile boundaries |
| Executor | Mock repository returning subject + corpus rows; assert columns present only when flag set |
| Integration (optional) | Small fixture DB: two players, known ordering, assert percentile |

**Manual validation:** Run `AverageMaterialVolatility` for Fischer with `includeCorpusBenchmark:
true` — confirm `CorpusPercentile` and `DeltaFromCorpus` move in sensible direction vs a second
player (e.g. Petrosian) on the same filters.

#### 12.7.5 Non-goals (v1)

- Persisted benchmark cache tables
- External reference-player presets (user compares by running two queries or future `playerB` filter)
- Benchmarks on non-style metrics (`GameCountByEco`, etc.)
- Default `includeCorpusBenchmark: true` until Phase 3 metrics exist (optional follow-up)

---

## 13. Risks and mitigations

| Risk | Mitigation |
|------|------------|
| Move derivation fails on edge games | Tests + soft-fail (skip row, log); optional `GameMoveQuality` column later. |
| Large DB backfill time | Process game-by-game; optional parallelism with cap. |
| Transaction size for huge games | Batch inserts inside per-game transaction or chunk by N plies (rare games > 500 plies). |
| Unauthenticated metrics HTTP endpoint | **Accepted for local-only solo use** (see §12.1, §12.4). Before **deployment or network exposure**, add rate limits, auth, or strict network restriction and document the chosen approach. |
| Misleading benchmark percentiles on tiny corpora | Require `benchmarkMinGames`; return `CorpusEligiblePlayerCount`; document corpus-local interpretation (DESIGN §12.8). |

---

## 14. Out of scope (product backlog — not §12 v1)

- “Material gained since opening” as a separate metric (DESIGN §9).
- Unknown-year bucket (DESIGN §9).
- Arbitrary SQL / raw table dumps from HTTP clients (keep **registered metrics + parameterized repository reads** only).
- **Engine-based style metrics** (ACPL, sharpness/WDL, sound sacrifice classification) — deferred;
  see [STYLE_METRICS.md](./STYLE_METRICS.md) and PLAN §12.6 Phase 9.

---

*End of PLAN.md. Stage 3 (§11) is complete; Stage 4 **§12** is complete. **Active backlog:** §12.7 corpus benchmarks, then §12.6 Phase 3; §12.5 player material comparison is complete.*
