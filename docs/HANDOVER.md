# ChessAnalyser — agent handover (analytics / DPI)

**Purpose:** Orient a **new** Cursor agent (or human) so work continues without re-deriving context from chat history.

**Last updated:** 2026-05-04 (analytics DPI track).

---

## 1. What this repo is

**ChessAnalyser** — .NET 8 solution: load PGN → parse → resolve players → compute **bitboard** positions per ply → persist to SQL Server (`Game`, `BoardPosition`, `Player`, parse errors). UI/host under `src/Analyser`.

---

## 2. Cursor Skill (DPI — global)

The DPI workflow is a **personal (global) skill** so it applies in **any** repository:

- Path: `~/.cursor/skills/dpi-workflow/SKILL.md` (on Windows: `C:\Users\<you>\.cursor\skills\dpi-workflow\SKILL.md`)
- Skill **name:** `dpi-workflow`
- Invoke via **@dpi-workflow** / Skills UI, or mention **DPI** in the prompt so the agent loads it.

ChessAnalyser-specific rules remain in **this repo’s** `DESIGN.md` / `PLAN.md`; the global skill only enforces the **process** (80/20, DESIGN → PLAN → IMPLEMENT).

---

## 3. Authoritative documents (read these first)

| Document | Role |
|----------|------|
| [DESIGN.md](../DESIGN.md) (repo root) | Functional + NFR requirements; **locked** stakeholder decisions (facts, rollups, C# vs SQL, year handling, internal API). |
| [PLAN.md](../PLAN.md) (repo root) | Implementation plan: migrations `006`–`008`, new services, ETL wiring, metrics, testing, **§11 checklist**. |

For other topics (coverage, audits), see `docs/*.md` as needed.

---

## 4. Where the analytics work left off

**Stages completed**

- **DESIGN** — Done and revised with answers to pushback questions (move as secondary fact, ply = half-move, incomplete dates omitted from year metrics, rollups yes, domain logic in C#, internal-only v1, explicit `Game` columns).
- **PLAN** — Done; detailed schema and application-layer breakdown.

**Stage not started (as of this handover)**

- **IMPLEMENT** — No `006`–`008` migration scripts have been added yet; no `GameMove` / `GamePositionSummary` tables; no analytics services/metrics wired in. Treat [PLAN.md §11 checklist](../PLAN.md) as the **source of truth** for remaining work.

If you implement part of the checklist, **update this section** (or replace with a dated note) so the next agent sees accurate status.

---

## 5. Technical snapshot (baseline before analytics code)

- **`dbo.Game`:** `Name`, `GameId`, `Winner`, `WhitePlayerId`, `BlackPlayerId`. PGN **tags are not stored** in SQL today — only on the in-memory `Game` during parse ([PLAN.md](../PLAN.md) §3).
- **`dbo.BoardPosition`:** `(GameId, PlyIndex)` PK; `PlyIndex = -1` initial; `0,1,2,…` after each half-move; 12 × `BIGINT` bitboards.
- **ETL chain:** `EtlService` → `PlayerResolver` → `BoardPositionService.SetBoardPositions` → `PersistenceService.InsertGames` → `ChessRepository.InsertGame` + `InsertBoardPositions`.

---

## 6. Conventions to preserve (from DESIGN)

- **Primary fact:** `BoardPosition`. **Move fact:** derived in **C#** from consecutive positions, persisted for frequency queries.
- **Aggregation unit for moves:** one **ply** (not full move).
- **`GameYear`:** nullable; **NULL** ⇒ exclude from any year-based analysis.
- **Chess logic:** C# only; SQL for persistence and simple relational reads/filters (see DESIGN §8.5).

---

## 7. Suggested first actions for the next implementer

1. Open [PLAN.md §11](../PLAN.md) and start with migration `006_AddGameAnalyticsColumns.sql`.
2. Extend `Game` DTO + `InsertGame` / repository to persist header fields mapped from `Tags` (year parsing per PLAN).
3. Proceed through checklist in order unless a dependency forces a small reorder.

---

## 8. Maintenance

- After a meaningful milestone, refresh **§4** in this file and any checklist in `PLAN.md`.
- When DESIGN/PLAN change, update traceability tables in those files rather than duplicating full requirements here.
