# ChessAnalyser — agent context (session handoff)

**Purpose:** Let a **new** chat or agent continue without re-reading full history. Update this file when you finish a meaningful slice of work.

**Last updated:** 2026-05-10 (PLAN §11 item 10: `IMetricRegistry` + two reference metric executors + repository reads + tests).

---

## 1. DPI documents (authoritative)

All **Design / Plan / Implement** specs for **board-position analytics** live in **`docs/`**:

| File | Role |
|------|------|
| [DESIGN.md](./DESIGN.md) | Requirements and locked decisions (facts, dimensions, C# vs SQL, rollups, year rules). |
| [PLAN.md](./PLAN.md) | Implementation plan and **§11 checklist** (ordered tasks). |
| **AGENT_CONTEXT.md** (this file) | Current progress and **recommended next small step**. |

**Global process skill:** `~/.cursor/skills/dpi-workflow/` (`dpi-workflow`) — 80/20 DESIGN+PLAN vs IMPLEMENT; applies in any repo.

---

## 2. Product state (high level)

- **Done:** PGN parse → player resolution → bitboard positions per ply → persist **`Game`**, **`BoardPosition`**, **`Player`**, parse errors. GitHub Actions runs **`dotnet test`** on PRs.
- **Housekeeping added:** schema-history scaffolding (`src/Migrations/History/` + `tools/export-db-history.ps1`) to snapshot current SQL object DDL after migrations.
- **In progress (analytics):** **PLAN §11 items 1–10** done through metric registry and reference SQL-backed metrics. Next is **item 11** (optional backfill service / CLI).

---

## 3. Recommended next step (small slice)

**Do next:** [PLAN.md §11](./PLAN.md) item **11** — optional **`IAnalyticsBackfillService`** and a host entry point for games missing analytics rows.

**Why this order:** Backfill closes gaps when ETL skipped games or analytics failed silently; metrics already read persisted facts.

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
- [ ] 11–13. Per PLAN.md

**As of last update:** items **1–10** in source control; ensure DbUp has applied through `010` if you use analytics tables locally.

---

## 5. Technical snapshot

- **`dbo.Game`:** plus **`Event`**, **`Site`**, **`DateTag`**, **`GameYear`**, **`Eco`** — populated on insert via **`PgnGameHeaderMapper`** from lowercase PGN tags (`event`, `site`, `date`, `eco`). **`GameYear`** is set only when the date looks like PGN `YYYY.MM…` with a four-digit year and no `?` in the year.
- **`dbo.BoardPosition`:** `(GameId, PlyIndex)`; `PlyIndex = -1` initial; half-moves `0,1,…`.
- **Rollups (in code):** **`ClassicalPieceValues`** + **`GamePositionSummaryFactory`** produce **`GamePositionSummary`** DTOs; **`ChessRepository.ReplaceGamePositionSummariesForGame`** persists them (orchestration in item 9).
- **Move facts:** **`GameMoveDeriver`** → **`GameMoveFact`**; **`ChessRepository.ReplaceGameMovesForGame`** persists rows (orchestration in item 9).
- **Conventions:** See [PLAN.md §7](./PLAN.md) and [DESIGN.md §8](./DESIGN.md).

---

## 6. Maintenance

- After each session: bump **Last updated**, refresh **§3** / **§4**, and tick items in **`docs/PLAN.md` §11** when done.
- Renamed from `HANDOVER.md` → **`AGENT_CONTEXT.md`** (2026-05-10); update any bookmarks or external links.
