# ChessAnalyser — Board-position analytics framework (DESIGN)

**Location:** This file lives in **`docs/`** with [PLAN.md](./PLAN.md) and [AGENT_CONTEXT.md](./AGENT_CONTEXT.md).  
**Document status:** Stage 1 revised — stakeholder answers to §8 incorporated. **Stage 2:** see [PLAN.md](./PLAN.md). **Session handoff:** [AGENT_CONTEXT.md](./AGENT_CONTEXT.md). **Global DPI skill:** `~/.cursor/skills/dpi-workflow/` (name: `dpi-workflow`).  
**Scope:** Analytics and statistics over **persisted** board positions (bitboards) and related dimensions, after PGN parse/load.  
**Out of scope for this design:** Changing the PGN parser, ETL orchestration, or persistence schema unless explicitly required for analytics.

---

## 1. Purpose and vision

Build a **flexible, extensible framework** for deriving **statistics and aggregations** from stored chess positions, treating the warehouse metaphor seriously:

| Role | In this system (initial framing) |
|------|-----------------------------------|
| **Primary fact** | `BoardPosition`: one row per `(GameId, PlyIndex)` with bitboards. |
| **Secondary fact** | **Move** (from/to, piece, capture, etc.): **derived** from consecutive primary rows (same game, adjacent ply); persisted for efficient move-frequency metrics. |
| **Dimensions** | Anything that slices or filters facts: player (white/black), colour (as side or as piece colour), calendar time (year, decade), game metadata (ECO, result, event), ply / fullmove number, piece type, etc. |

The framework should make it **easy to add new metrics** and **efficient to run common queries** without rewriting core plumbing each time.

---

## 2. Stakeholder goals

1. **Analysts / product:** Ask questions like “average material by white player in 1938” or “how often does a knight land on e5 in games by player X” without bespoke SQL each time.
2. **Engineering:** Clear extension points, testable components, bounded complexity (no “one giant script” as the only interface).
3. **Operations:** Predictable resource use (CPU, memory, I/O) on realistic data volumes.

---

## 3. Functional requirements

### 3.1 Core analytics capabilities

| ID | Requirement | Notes |
|----|-------------|--------|
| F-1 | **Board-state metrics (“material” and counts)** | At each ply, support metrics over **what is on the board**: total piece value by colour, **per-piece-type counts** (e.g. knights on the board), and any combination of dimensions (player, year, ply index, colour, etc.). Examples: “at ply 4, average total material for White’s side for player X”; “at ply 4, average number of knights, globally and split White/Black.” **Configurable** piece values for value-based totals. |
| F-2 | **Move / piece-frequency metrics** | Support statistics on **how often** certain events occur: e.g. piece type P moved to square S, captures, promotions (if derivable), etc., filterable by player, year, colour, ECO, etc. |
| F-3 | **Dimensional filtering** | All public metrics must support **combinations** of filters (e.g. `Year = 1938 AND WhitePlayer = "Capablanca, Jose Raul" AND PlyIndex BETWEEN 0 AND 40`). |
| F-4 | **Aggregation** | Support **COUNT**, **SUM**, **AVG**, **MIN**, **MAX** where semantically valid; extensible registry for custom reducers if needed later. |
| F-5 | **Grouping** | Support **GROUP BY** one or more dimensions (year, player, ECO bucket, decile of ply, etc.). |
| F-6 | **Extensibility** | New statistic types should be addable **without** modifying unrelated metrics (plugin-like or registry pattern; see §6). |
| F-7 | **Reproducibility** | Same inputs (DB snapshot + metric definition + parameters) → same outputs (deterministic implementation; document rounding if any). |
| F-11 | **Corpus-relative benchmarks** | Per-player style metrics should optionally report **how a filtered player compares to other players in the same database slice** (same non-identity filters), not only a raw scalar. See §12. |

### 3.2 Data access and interfaces

| ID | Requirement | Notes |
|----|-------------|--------|
| F-8 | **Immutability of primary facts** | **Query** execution must not mutate `Game`, `BoardPosition`, `Player`, or other **primary** ingest facts. **Rollup / move-fact** tables may be written only by **explicit batch jobs** (post-load or on-demand), not as a side effect of ad-hoc reads. |
| F-9 | **Programmatic API** | **Primary:** strongly-typed **metric + filters** in C# (`IMetricRegistry`, `AnalyticsQuery`, `AnalyticsTableResult`) for services and tests. **Optional:** the `Analyser` host exposes read-only JSON for **registered metrics only** under `/api/analytics/metrics` (PLAN §12); treat as **integrator / dev** surface until authentication and rate limits are applied. |
| F-10 | **Rollups / secondary facts** | **Required direction of travel:** summary and rollup tables (and the **derived move** fact table) populated **after load** (or on demand) so reports do not always scan raw bitboards. Exact tables belong in PLAN.md. |

### 3.3 Dimensions (initial catalogue)

Dimensions should be **first-class** in the design even if not all are implemented in v1.

| Dimension | Source (conceptual) | Resolution / notes |
|-----------|---------------------|---------------------|
| Game | `Game` | GameId, Name, Result/Winner, etc. |
| White player / Black player | `Game.WhitePlayerId`, `BlackPlayerId` → `Player` | Ingest-time resolution stores canonical IDs; public filters use player surname/forenames so users do not need database IDs. |
| Calendar **year** | **Explicit column** on `Game` (see §3.5) | Games with **incomplete/unknown** PGN dates are **omitted** from any analysis or grouping that depends on year (no “unknown year” bucket unless added later as a separate requirement). |
| Ply | `BoardPosition.PlyIndex` | **Unit of aggregation for moves and position stats: one ply** (half-move). Aligns with colour/player differentiation; not “full move” (White+Black). |
| Colour / side | Derived from schema + ply convention | Document in PLAN/implementations and tests (NFR-5). |
| Piece type / square | Bitboards at primary fact; move rows for “piece moved to S” | Mitigate cost via rollups and derived **move** fact (§3.4). |
| ECO, Event, Site, … | **Explicit columns** on `Game` where needed for filters | §3.5 — indexed, filterable fields; not inferred from free text in v1. |

### 3.4 Fact model (locked)

1. **Primary fact:** `BoardPosition` only — authoritative snapshot per `(GameId, PlyIndex)`.
2. **Secondary fact — move:** A **first-class persisted move row** (conceptually move-at-ply), **derived in application code** from `(position at ply-1, position at ply)` for the same game. Used for move-frequency and similar metrics without recomputing diffs on every query.
3. **Aggregation unit for move statistics:** **One ply** (not one full move).

### 3.5 Schema: explicit game metadata (locked)

`Game` (and migrations) shall carry **explicit, indexable columns** for analytics dimensions — including but not limited to **year** (nullable; null = excluded from year-based analysis), **ECO**, **WhitePlayerId** / **BlackPlayerId** (already present), and other header fields required for filters. **No reliance** on parsing `Game.Name` or opaque tag blobs for v1 analytics dimensions.

---

## 4. Non-functional requirements

### 4.1 Performance and scale

| ID | Requirement |
|----|-------------|
| NFR-1 | Queries over **large** `BoardPosition` sets must use **indexed** dimensions (`GameId`, `PlyIndex`, dates, player ids) and avoid full table scans where possible. |
| NFR-2 | **Chess semantics** (bitboard interpretation, diffs, material, piece counts) run in **C#**. Performance comes from **bounded I/O** (filters on indexed columns), **streaming/batching**, **rollup/secondary tables** populated by C#, and **avoiding** loading the entire position corpus into memory for one ad-hoc query. |
| NFR-3 | Document **expected data volume** (games, plies per game) and target query latency (e.g. p95 &lt; X s for defined benchmark queries). |

### 4.2 Correctness

| NFR-4 | **Material valuation** must use a **single configurable policy** (e.g. Q=9, R=5, …) versioned or named so reports don’t mix incompatible numbers. |
| NFR-5 | **Colour / side** definitions must be documented and tested (especially for “material by colour” vs “by player”). |

### 4.3 Security and operations

| NFR-6 | Analytics SQL uses **parameterized** queries; no string-concatenated user filter values. |
| NFR-7 | Connection strings and credentials follow existing app patterns. |

### 4.4 Maintainability

| NFR-8 | Each metric has a **short specification** (inputs, dimensions, formula, edge cases). |
| NFR-9 | Unit tests for **pure functions** (bitboard → metrics, move derivation, ply conventions); integration tests optional against a small DB for **read paths and rollup jobs**. |

---

## 5. Architectural principles (locked + recommendation)

1. **Domain logic in C#:** All **chess-relevant** and **metric** computation (bitboards, move derivation, material, counts, reducers) lives in **C#**. The database is for **durable storage**, **indexed retrieval**, and **simple relational access** (see §8.5 for the narrow exception).
2. **Layering:** Metric registry / executors → repositories (read-only for queries; write paths for rollup jobs) → persistence. No ad-hoc analytics scattered only in UI.
3. **Rollups and secondary facts:** Use **rollup tables** and the **derived move** fact table (§3.4) to keep interactive queries tractable; **populate/update in C#** after ingest or on schedule.
4. **Explicit dimensions on `Game`:** Indexed columns per §3.5; migrations as needed.
5. **Extensibility:** “Framework” = **metric definitions** + **registry of C# executors** + **shared dimension vocabulary**, not one-off scripts per report.

---

## 6. Extensibility model (sketch)

- **MetricDefinition:** name, description, supported dimensions, required facts/rollups, optional parameters (e.g. piece values profile).
- **IMetricExecutor:** C# implementation: `ExecuteAsync(AnalyticsQuery query, CancellationToken ct)` returning tabular result (rows + typed columns).
- **Registry:** register executors at startup; validate query against allowed dimensions/filters.

Detailed interfaces belong in PLAN.md; this section captures the **design intent**.

---

## 7. Out of scope (unless promoted later)

- Real-time streaming analytics during PGN load (separate from batch analytics).
- Opening book / engine evaluation (centipawns) — different fact source.
- Multi-tenant security (unless required).

---

## 8. Resolved decisions (stakeholder answers)

| Ref | Topic | Decision |
|-----|--------|----------|
| **Q1** | Primary vs move fact | **Primary fact** remains `BoardPosition`. **Move** is a **secondary**, **persisted** fact **derived** from consecutive board rows (application-layer derivation). |
| **Q2** | Move-frequency unit | **One ply** (half-move), consistent with colour/player slicing — not “full move.” |
| **Q3** | Incomplete PGN dates | **Omitted** from any metric or dimension that depends on **year** (no inference from event text in v1). |
| **Q4** | “Material” / board metrics | Metrics over **state on the board** at a ply: total values, counts by piece type, splits by White/Black, combinable with dimensions (e.g. player, ply, year when present). “Material gained since start” is **out of scope** unless later added as a **separate** metric. |
| **Q5** | Rollups | **Yes** — summary/rollup tables (and derived move storage) are **in scope**. |
| **Q6** | SQL vs C# | **Chess and analytics logic in C#** (see §8.5). |
| **Q7** | API | **Primary:** internal C# contracts for v1. **Supplementary:** small REST JSON surface on `Analyser` for metric discovery and execution is **in scope** as an optional consumer path (not a replacement for the internal API). |
| **Q8** | `Game` schema | **Yes** — **explicit columns** on `Game` for year, ECO, players, etc., for indexing and filters. |

### 8.5 SQL vs C# — design stance (Q6)

**Your preference:** all **meaningful** logic in **C#**, not in the database layer.

**Agreement:** For this project, that is **coherent and acceptable**, especially for solo use: one language for rules, strong unit tests, and clear boundaries. The **trade-off** is that the team (you) must enforce performance through **data shape** (rollups, move table, indexed `Game` columns), **streaming/batching**, and **avoiding** “read every bitboard for every query.”

**Recommended narrow exception (storage, not chess):** allow SQL to perform **relational filtering** (`WHERE` on indexed columns) and **trivial scalar aggregates** on **already-summarized** rollup rows (e.g. `SUM(Count)`), without encoding chess rules in T-SQL. If you prefer **even those** final aggregates in C#, that remains compatible with DESIGN at the cost of slightly more application code. **Chess semantics** (bitboards, diffs, material definitions) stay **strictly out of T-SQL**.

---

## 9. Open items (non-blocking for PLAN, minor)

- Optional future metric: **material gained** vs opening (explicitly separate from F-1).
- Optional **“unknown year”** bucket if you later want visibility into games dropped from year reports.
- **Playing-style metrics** (move/position fingerprints without engine evaluation): research in
  [STYLE_METRICS.md](./STYLE_METRICS.md); implementation sequence in [PLAN.md §12.6](./PLAN.md).
- **Corpus benchmarks for style metrics:** design in §12; implementation in [PLAN.md §12.7](./PLAN.md).

---

## 10. Success criteria (acceptance-level)

1. At least **two** implemented reference metrics: e.g. (a) **average board-state value / counts by colour and year** (games without year excluded from that slice), (b) **frequency of knight moves to a selectable square** (or equivalent), both through the same framework API.
2. Adding a **third** metric requires only **new metric class + registration** (or documented equivalent), not rewiring the entire pipeline.
3. Documented **performance note** (e.g. batch sizes, rollup usage, rough timings) for reference workloads on a stated data size.
4. DESIGN + PLAN updated when scope changes.

---

## 11. Traceability

| This DESIGN section | Informs PLAN.md |
|---------------------|-----------------|
| §3 Functional | Task breakdown, libraries, migrations |
| §4 NFR | Testing, indexing, performance tasks |
| §8 Resolved decisions | Schema, jobs, executor boundaries |
| §12 Corpus benchmarks | PLAN §12.7 |

---

## 12. Corpus-relative benchmarks (locked direction)

### 12.1 Problem

Raw per-player style scalars (e.g. `AverageMaterialVolatility = 1.34` for Fischer over 827 games)
are **hard to interpret in isolation**. External tools (Chessiverse, ChessBase style reports, academic
typology work) normalize player features against a **reference population** before presenting style
labels or rankings.

ChessAnalyser should do the same, but **honestly**: benchmarks are always **relative to the loaded
corpus and active filters**, not universal chess history.

### 12.2 What a benchmark is (and is not)

| In scope | Out of scope |
|----------|----------------|
| **Corpus average** — mean of the same metric across eligible players in the DB slice | Fixed external GM norms (“Tal = 1.8 volatility”) |
| **Delta from corpus** — subject value minus corpus average | Engine-based quality or ACPL |
| **Percentile rank** — subject’s position in the corpus distribution (0–100) | Multi-tenant or cross-database normalization |
| Same **non-identity** filters as the subject query (year, ECO, colour mode, ply window) | Benchmarks that ignore filter alignment |

A benchmark answers: *“Compared with everyone else in **my** database under **these** filters, is
this player high, low, or typical on this metric?”*

### 12.3 Reference population (locked)

1. Apply the metric’s **per-player** aggregation formula to **every player** who appears in games
   matching the query’s **non-identity** filters (`minGameYear`, `maxGameYear`, `eco`, `playerColour`
   when it narrows the game set, `minPlyIndex` / `maxPlyIndex`, etc.).
2. **Exclude the subject player** from the corpus average and percentile calculation (leave-one-out
   style) so self-inclusion does not bias the baseline when the subject has many games.
3. Include only players with at least **`benchmarkMinGames`** appearances in the filtered slice
   (default **30**; configurable on the query).
4. The **subject row** is always returned when `playerSurname` is set, even if below
   `benchmarkMinGames`; benchmark columns may be null with a documented reason when the corpus has
   too few eligible players.

### 12.4 Query contract (proposed)

Extend **`AnalyticsQuery`** (PLAN §12.7):

| Field | Type | Default | Purpose |
|-------|------|---------|---------|
| `includeCorpusBenchmark` | `bool?` | `false` for existing metrics until migrated; **`true` default for style metrics** once supported | When true, append benchmark columns. |
| `benchmarkMinGames` | `int?` | `30` | Minimum games per player for inclusion in corpus distribution. |

**Backward compatibility:** existing API consumers that omit `includeCorpusBenchmark` keep receiving
today’s column set until they opt in (or until a documented migration window).

### 12.5 Result shape (proposed)

When `includeCorpusBenchmark = true`, append columns to the **single subject row** (not a second
“All players” row — unlike `AverageMaterialByPlayerAtMove`, which uses explicit series rows):

| Column | Meaning |
|--------|---------|
| `CorpusAverage` | Mean per-player metric value over eligible corpus players |
| `DeltaFromCorpus` | Subject value − `CorpusAverage` |
| `CorpusPercentile` | Percentile rank 0–100 within eligible corpus (higher = higher metric value) |
| `CorpusEligiblePlayerCount` | Players with ≥ `benchmarkMinGames` in the slice |

Keep the existing subject columns (`GameCount`, `AverageMaterialVolatility`, etc.) unchanged.

**Rounding:** document in metric specs; suggest 2 decimal places for averages/deltas, 1 for
percentile in presentation.

### 12.6 Priority metrics

Implement benchmarks **first** on metrics where raw units are opaque:

1. `AverageMaterialVolatility` (proof of concept)
2. `BishopPairFrequency`, `MinorPieceComposition`
3. `AverageCastlingPly`

New style metrics from PLAN §12.6 Phase 3 onward should ship **with** benchmark support when
practical, using shared enrichment code.

### 12.7 Architecture (sketch)

- **Shared helper** (name flexible): given subject per-player value + SQL or repository method that
  returns corpus distribution for the same metric definition and filters → compute
  `CorpusAverage`, `DeltaFromCorpus`, `CorpusPercentile`.
- Reuse the **per-player aggregation SQL** already used for the subject; wrap in an outer query or
  C# pass over per-player rows — prefer one repository round-trip per metric where §8.5 allows.
- **No new persisted tables** in v1; benchmarks are computed on read.
- Optional v2: cache corpus aggregates per `(metricKey, filter hash)` if performance requires it.

Pattern precedent: **`AverageMaterialByPlayerAtMove`** already compares Player A to an all-player
baseline; corpus benchmarks generalize that idea for style metrics with percentile semantics.

### 12.8 Interpretation and UI copy

Documentation and `MetricCatalog` hints must state:

- Percentiles are **corpus-local** (“78th percentile in this database with these filters”).
- High/low is **not** good/bad — style fingerprint only.
- Sparse corpora (few players or few games) produce unreliable benchmarks; surface
  `CorpusEligiblePlayerCount`.

See also [STYLE_METRICS.md §8](./STYLE_METRICS.md).

---

*Implementation references [PLAN.md](./PLAN.md) in this folder. Update [AGENT_CONTEXT.md](./AGENT_CONTEXT.md) when milestones change.*
