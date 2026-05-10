# ChessAnalyser — agent context (session handoff)

**Purpose:** Let a **new** chat or agent continue without re-reading full history. Update this file when you finish a meaningful slice of work.

**Last updated:** 2026-05-10 (migration `006` implemented in repo).

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
- **In progress (analytics):** DESIGN and PLAN are complete; **PLAN §11 item 1** is implemented (`006_AddGameAnalyticsColumns.sql` + `Migrations/README.md`). ETL still does not populate the new `Game` columns — next is **items 4–5** (DTO, `InsertGame`, header mapping from `Tags`).

---

## 3. Recommended next step (small slice)

**Do next:** [PLAN.md §11](./PLAN.md) items **4–5** — extend **`Game`** DTO, **`InsertGame`** / `ChessRepository`, and map PGN **`Tags`** → `Event`, `Site`, `DateTag`, `GameYear`, `Eco` in one place before insert.

**Then:** run **DbUp** if you have not yet applied `006` on your database; item `007` / `008` remain future PRs.

**Why this order:** Schema for analytics dimensions exists; application must persist them during load.

---

## 4. Checklist mirror (PLAN §11)

Sync this subsection when items complete (or rely on `PLAN.md` checkboxes only — but then tick **PLAN.md** in git).

- [x] 1. `006_AddGameAnalyticsColumns.sql` + Migrations README (run DbUp locally to verify against your DB)
- [ ] 2. `007_CreateGameMoveTable.sql`
- [ ] 3. `008_CreateGamePositionSummaryTable.sql`
- [ ] 4–13. Per PLAN.md

**As of last update:** item **1** done in source control; confirm DbUp on your environment.

---

## 5. Technical snapshot

- **`dbo.Game`:** core columns as before; plus nullable **`Event`**, **`Site`**, **`DateTag`**, **`GameYear`**, **`Eco`** (migration `006`). Values stay **NULL** until ETL maps `Tags` and `InsertGame` is extended ([PLAN §11](./PLAN.md) items 4–5).
- **`dbo.BoardPosition`:** `(GameId, PlyIndex)`; `PlyIndex = -1` initial; half-moves `0,1,…`.
- **Conventions:** See [PLAN.md §7](./PLAN.md) and [DESIGN.md §8](./DESIGN.md).

---

## 6. Maintenance

- After each session: bump **Last updated**, refresh **§3** / **§4**, and tick items in **`docs/PLAN.md` §11** when done.
- Renamed from `HANDOVER.md` → **`AGENT_CONTEXT.md`** (2026-05-10); update any bookmarks or external links.
