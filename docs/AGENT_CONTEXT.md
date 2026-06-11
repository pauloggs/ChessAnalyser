# ChessAnalyser — agent context (session handoff)

**Purpose:** Let a **new** chat or agent continue without re-reading full history. Update this file when you finish a meaningful slice of work.

**Last updated:** 2026-06-11 (`CastlingSidePreference` metric — PR in flight.)

---

## 1. DPI documents (authoritative)

All **Design / Plan / Implement** specs for **board-position analytics** live in **`docs/`**:

| File | Role |
|------|------|
| [DESIGN.md](./DESIGN.md) | Requirements and locked decisions; **§12** corpus benchmarks (F-11). |
| [PLAN.md](./PLAN.md) | Implementation plan; **§12.6** (style metrics), **§12.7** (corpus benchmarks). |
| [STYLE_METRICS.md](./STYLE_METRICS.md) | Style research, literature, metric catalogue, profile combinations, caveats. |
| **AGENT_CONTEXT.md** (this file) | Current progress and **recommended next small step**. |

**Global process skill:** `~/.cursor/skills/dpi-workflow/` (`dpi-workflow`) — 80/20 DESIGN+PLAN vs IMPLEMENT; applies in any repo.

---

## 2. Product state (high level)

- **Done:** PGN parse → player resolution → bitboard positions per ply → persist **`Game`**, **`BoardPosition`**, **`Player`**, parse errors. GitHub Actions runs **`dotnet test`** on PRs.
- **Done (analytics groundwork):** **PLAN §11** (items 1–13) and **§12** (metrics HTTP API + local **`wwwroot`** UI).
- **Done (style metrics Phases 1–3):** `AverageCastlingPly`, `AverageMaterialVolatility`, `BishopPairFrequency`, `MinorPieceComposition`, `CaptureRate`, `QueenTradeRate`.
- **Done (corpus benchmarks on main):** `AverageMaterialVolatility`, `AverageCastlingPly`, `BishopPairFrequency`, `MinorPieceComposition`, `CaptureRate` (PR #56).
- **Done (corpus benchmarks):** `QueenTradeRate` (PR #57).
- **Done (Phase 4):** `CentreMoveRate` (PR #58).
- **Done (Phase 4):** `ForwardMoveRate` (PR #59).
- **In flight:** `CastlingSidePreference` (Phase 5).
- **HTTP auth for metrics is deferred** while the app stays **local-only / undeployed** (see PLAN §12.1 / §12.4).

---

## 3. Workflow — when the user says **“next step”**

**Always follow this sequence (one PR-sized slice per step):**

1. **`git fetch origin`** and **`git checkout main && git pull origin main`** — start from latest merged `main` only.
2. **`git checkout -b feat/<short-description>`** — new branch for this slice only.
3. **Implement** the next unchecked item in [PLAN.md](./PLAN.md) (or the slice named in §3.1 below).
4. **`dotnet test`** — must pass before push.
5. **Commit, push, `gh pr create`** — one PR per step; do **not** stack unrelated metrics on the same branch.
6. **Update this file** (§2 / §3.1) in the same PR when the slice changes backlog state.

**Do not:** branch from an older feature branch, open a second PR while the first is unmerged, or implement the next metric before the previous PR is on `main` (avoids merge conflicts in shared files: executors, `SqlStatements.cs`, `index.html`, tests).

### 3.1 Recommended next step (small slice)

**Do next (after CastlingSidePreference PR merges):** [PLAN.md §12.6 Phase 5](./PLAN.md) — implement **`OppositeSideCastlingRate`**.

**Then:** `UncastledKingRate`, Phase 6+ per PLAN; corpus benchmarks on Phase 4 metrics as follow-up PRs.

**Do not prioritize yet:** HTTP auth / rate limits for metrics (PLAN §12.4 / §13).

---

## 4. Checklist mirror (PLAN §11)

- [x] Items 1–13 — see [PLAN.md §11](./PLAN.md)
- [x] PLAN §12 metrics HTTP API (Stage 4)

---

## 5. Technical snapshot

- **`dbo.GameMove`** + **`dbo.GamePositionSummary`** — derived on ETL/backfill; required for style metrics.
- **Corpus benchmarks:** opt-in `includeCorpusBenchmark` + `benchmarkMinGames` (default 30); shared `ICorpusBenchmarkCalculator`.
- **Conventions:** [PLAN.md §7](./PLAN.md), [DESIGN.md §8](./DESIGN.md).

---

## 6. Maintenance

- After each session: bump **Last updated**, refresh **§2** / **§3.1**, tick **PLAN.md** when items complete.
