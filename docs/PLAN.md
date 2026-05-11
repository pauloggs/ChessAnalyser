# ChessAnalyser — Board-position analytics (PLAN)

**Location:** **`docs/`** — alongside [DESIGN.md](./DESIGN.md) and [AGENT_CONTEXT.md](./AGENT_CONTEXT.md).  
**Document status:** Stage 2 (design guide) + **Stage 3 complete** (§11 checklist, 2026-05-10) + **Stage 4 §12 checklist complete** (metrics HTTP API + DESIGN F-9 / Q7 alignment). **Active implementation direction:** §12.4 (first: **unified `wwwroot` web UI** for local daily use; then metrics catalog extension; HTTP auth **deferred** while the app stays local-only / undeployed).  
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
| Execute | **`POST /api/analytics/metrics/execute`** (or `…/run`) with body `{ "metricKey": "…", "query": { … } }` mapping to **`AnalyticsQuery`** (`MinGameYear`, `MaxGameYear`, `WhitePlayerSurname`, `WhitePlayerForenames`, `BlackPlayerSurname`, `BlackPlayerForenames`, `Eco`, `SummaryPlyIndex`). |
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

1. **Unified local web UI (`wwwroot`)** — make the static app the **primary** surface for solo use: PGN path, **LoadGames**, progress polling, **CancelLoad** (already partly on `index.html`); add sections that call existing JSON APIs for **metrics discovery + execute** (`GET/POST /api/analytics/metrics/…`) and **paged GetGames** (`GET /Analyser/GetGames` with filters) so routine work does not require Swagger. Keep **Swagger** as an optional dev “API docs” link, not part of the default workflow.
2. [x] **Extend the metrics surface** — add **`IMetricExecutor`** implementations for additional questions you care about (same patterns as the two reference metrics: parameterized repository reads, tabular `AnalyticsTableResult` only). Register them in **`IMetricRegistry`** / DI. Current additions include `GameCountByEco`, `AverageMaterialByPlayerAtMove`, `GameCountByYear`, `GameCountByResult`, `GameCountByPlayer`, and `PlayerResultSummary`.
3. [x] **Improve discovery** — enrich **`GET /api/analytics/metrics`** with clearer descriptions and parameter hints so the web UI and Swagger stay usable as the catalog grows.
4. **Tests** — per-metric executor tests with mocked **`IChessRepository`** (and API smoke tests for new keys), following §12.3; add light **Playwright** or manual test notes for the unified web UI if you introduce non-trivial client script.

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

## 13. Risks and mitigations

| Risk | Mitigation |
|------|------------|
| Move derivation fails on edge games | Tests + soft-fail (skip row, log); optional `GameMoveQuality` column later. |
| Large DB backfill time | Process game-by-game; optional parallelism with cap. |
| Transaction size for huge games | Batch inserts inside per-game transaction or chunk by N plies (rare games > 500 plies). |
| Unauthenticated metrics HTTP endpoint | **Accepted for local-only solo use** (see §12.1, §12.4). Before **deployment or network exposure**, add rate limits, auth, or strict network restriction and document the chosen approach. |

---

## 14. Out of scope (product backlog — not §12 v1)

- “Material gained since opening” as a separate metric (DESIGN §9).
- Unknown-year bucket (DESIGN §9).
- Arbitrary SQL / raw table dumps from HTTP clients (keep **registered metrics + parameterized repository reads** only).

---

*End of PLAN.md. Stage 3 (§11) is complete; Stage 4 **§12** is complete; follow **§12.4** for general metrics/UI work and **§12.5** for the planned player material comparison sequence until deployment plans change.*
