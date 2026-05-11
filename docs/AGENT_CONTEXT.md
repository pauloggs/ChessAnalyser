# ChessAnalyser — agent context (session handoff)

**Purpose:** Let a **new** chat or agent continue without re-reading full history. Update this file when you finish a meaningful slice of work.

**Last updated:** 2026-05-11 (PLAN **§12.4** item 2: `GameCountByResult` metric added; HTTP auth **deferred** — see [PLAN.md §12.4](./PLAN.md).)

---

## 1. DPI documents (authoritative)

All **Design / Plan / Implement** specs for **board-position analytics** live in **`docs/`**:

| File | Role |
|------|------|
| [DESIGN.md](./DESIGN.md) | Requirements and locked decisions (facts, dimensions, C# vs SQL, rollups, year rules). |
| [PLAN.md](./PLAN.md) | Implementation plan; **§11** (closed) + **§12** (metrics HTTP API) + **§12.4** (current suggested slice). |
| **AGENT_CONTEXT.md** (this file) | Current progress and **recommended next small step**. |

**Global process skill:** `~/.cursor/skills/dpi-workflow/` (`dpi-workflow`) — 80/20 DESIGN+PLAN vs IMPLEMENT; applies in any repo.

---

## 2. Product state (high level)

- **Done:** PGN parse → player resolution → bitboard positions per ply → persist **`Game`**, **`BoardPosition`**, **`Player`**, parse errors. GitHub Actions runs **`dotnet test`** on PRs.
- **Housekeeping added:** schema-history scaffolding (`src/Migrations/History/` + `tools/export-db-history.ps1`) to snapshot current SQL object DDL after migrations.
- **Done (analytics groundwork):** **PLAN §11** (items 1–13) and **§12** (metrics JSON on `Analyser`: `GET /api/analytics/metrics`, `POST /api/analytics/metrics/execute`). **Done (local UI/discovery):** unified **`wwwroot`** web experience for ETL + metrics + game browse, player material comparison UI, and metrics discovery descriptions plus parameter hints. **Done (metrics surface):** `GameCountByEco`, `AverageMaterialByPlayerAtMove`, `GameCountByYear`, and `GameCountByResult`. **HTTP auth for metrics is deferred** while the app stays **local-only / undeployed** (owner decision; see PLAN §12.1 / §12.4).

---

## 3. Recommended next step (small slice)

**Do next:** No fixed planned PR is queued. Choose the next concrete analysis question before adding another metric, or do a small cleanup/test-hardening slice if one is called out by the maintainer.

**Then:** Continue §12.4 item 4 tests as needed for each new metric. Optionally refresh [ANALYTICS_MATERIALIZATION_PERF.md](./ANALYTICS_MATERIALIZATION_PERF.md) if deriver/summary hot paths change.

**Do not prioritize yet:** Dedicated HTTP **auth / rate limits** for `AnalyticsMetricsController` — **out of scope** until there is a **deployment or network exposure** plan (then treat as blocking; update PLAN §12.1 / §13). Optional hygiene: bind the dev host to **localhost** only.

---

## 4. Checklist mirror (PLAN §11)

Sync this subsection when items complete (or rely on `PLAN.md` checkboxes only — but then tick **PLAN.md** in git).

- [x] 1. `006_AddGameAnalyticsColumns.sql` + Migrations README (run DbUp locally to verify against your DB)
- [x] 2. `007_CreateGameMoveTable.sql`
- [x] 3. `008_CreateGamePositionSummaryTable.sql`
- [x] 4–5. `Game` DTO + `InsertGame` + `PgnGameHeaderMapper` / `PersistenceService`
- [x] 6. `IPieceValues` + `IGamePositionSummaryFactory` + `GamePositionSummary`
- [x] 7. `GameMoveDeriver` + tests
- [x] 8. Repository `SqlStatements` + replace/read for `GameMove` and `GamePositionSummary`
- [x] 9. `IAnalyticsMaterializationService` + ETL wiring after board insert
- [x] 10. `IMetricRegistry` + reference metric executors + tests
- [x] 11. `IAnalyticsBackfillService` + Analyser `--backfill-analytics` CLI
- [x] 12. Materialization perf smoke (`docs/ANALYTICS_MATERIALIZATION_PERF.md`, `--profile-materialization`)
- [x] 13. `src/Migrations/README.md` analytics / tooling notes

### 4.1 PLAN §12 (Stage 4 — mirror)

- [x] 1–5. Metrics HTTP API + DESIGN F-9 / Q7 (see [PLAN.md §12](./PLAN.md))

**As of last update:** PLAN §11 items **1–13** and §12 checklist in source control; ensure DbUp has applied through `010` if you use analytics tables locally.

**CLI (Analyser, no web host):** `--backfill-analytics` (optional `--max-games N`); `--profile-materialization` (optional `--iterations N`, default 5000). Both exit after console output.

**HTTP (Analyser web host):** `GET /api/analytics/metrics`, `POST /api/analytics/metrics/execute` — **no auth** while local-only; **defer** auth/rate limits until deployment (PLAN §12.4 / §13); see controller remarks.

---

## 5. Technical snapshot

- **`dbo.Game`:** plus **`Event`**, **`Site`**, **`DateTag`**, **`GameYear`**, **`Eco`** — populated on insert via **`PgnGameHeaderMapper`** from lowercase PGN tags (`event`, `site`, `date`, `eco`). **`GameYear`** is set only when the date looks like PGN `YYYY.MM…` with a four-digit year and no `?` in the year.
- **`dbo.BoardPosition`:** `(GameId, PlyIndex)`; `PlyIndex = -1` initial; half-moves `0,1,…`.
- **Rollups (in code):** **`ClassicalPieceValues`** + **`GamePositionSummaryFactory`** produce **`GamePositionSummary`** DTOs; **`ChessRepository.ReplaceGamePositionSummariesForGame`** persists them (ETL + backfill via **`IAnalyticsMaterializationService`**).
- **Move facts:** **`GameMoveDeriver`** → **`GameMoveFact`**; **`ChessRepository.ReplaceGameMovesForGame`** persists rows (same path). CPU perf smoke: [ANALYTICS_MATERIALIZATION_PERF.md](./ANALYTICS_MATERIALIZATION_PERF.md) and **`--profile-materialization`** on Analyser.
- **Metrics HTTP:** **`AnalyticsMetricsController`** wraps **`IMetricRegistry`** (discovery + execute); internal executors unchanged.
- **Conventions:** See [PLAN.md §7](./PLAN.md) and [DESIGN.md §8](./DESIGN.md).

---

## 6. Maintenance

- After each session: bump **Last updated**, refresh **§3** / **§4**, and tick items in **`docs/PLAN.md` §11** when done.
- Renamed from `HANDOVER.md` → **`AGENT_CONTEXT.md`** (2026-05-10); update any bookmarks or external links.
