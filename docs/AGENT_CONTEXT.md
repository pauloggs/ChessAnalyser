# ChessAnalyser — agent context (session handoff)

**Purpose:** Let a **new** chat or agent continue without re-reading full history. Update this file when you finish a meaningful slice of work.

**Last updated:** 2026-05-10.

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
- **In progress (analytics):** DESIGN and PLAN are complete; **implementation has not started** (no `006`–`008` migrations, no analytics tables or services in code yet).

---

## 3. Recommended next step (small, first slice)

**Goal:** Start IMPLEMENT in **one vertical slice** without boiling the ocean.

**Do next (single PR or session):**

1. **Migration `006_AddGameAnalyticsColumns.sql` only** — idempotent `ALTER` on `dbo.Game` per [PLAN.md §4.1](./PLAN.md); optional indexes.
2. **Document in `src/Migrations/README.md`** — new script row.
3. **Run DbUp** locally and confirm the database matches.

**Defer to follow-up slices (in order):** checklist items 4–5 on [PLAN.md §11](./PLAN.md) (DTO + `InsertGame` + header mapping from `Tags`) **after** `006` is merged and stable — then `007` / `008`, then C# factories and deriver, then materialization wiring.

**Why this order:** Analytics dimensions (`GameYear`, `Eco`, …) must exist in SQL before ETL can persist them; doing **006 first** validates migrations and leaves the repo green before application changes.

---

## 4. Checklist mirror (PLAN §11)

Sync this subsection when items complete (or rely on `PLAN.md` checkboxes only — but then tick **PLAN.md** in git).

- [ ] 1. `006_AddGameAnalyticsColumns.sql` + Migrations README + DbUp verified
- [ ] 2. `007_CreateGameMoveTable.sql`
- [ ] 3. `008_CreateGamePositionSummaryTable.sql`
- [ ] 4–13. Per PLAN.md

**As of last update:** none of the above are done in the codebase.

---

## 5. Technical snapshot

- **`dbo.Game`:** `Name`, `GameId`, `Winner`, `WhitePlayerId`, `BlackPlayerId`. PGN **tags not in SQL** until header columns exist and ETL writes them ([PLAN §3](./PLAN.md)).
- **`dbo.BoardPosition`:** `(GameId, PlyIndex)`; `PlyIndex = -1` initial; half-moves `0,1,…`.
- **Conventions:** See [PLAN.md §7](./PLAN.md) and [DESIGN.md §8](./DESIGN.md).

---

## 6. Maintenance

- After each session: bump **Last updated**, refresh **§3** / **§4**, and tick items in **`docs/PLAN.md` §11** when done.
- Renamed from `HANDOVER.md` → **`AGENT_CONTEXT.md`** (2026-05-10); update any bookmarks or external links.
