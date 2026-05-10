# Analytics materialization — performance smoke (NFR-3)

**Purpose:** Satisfy [PLAN.md §11 item 12](./PLAN.md) and [DESIGN.md](./DESIGN.md) NFR-3 (document expected workload and a reproducible throughput check for the materialization path).

**What “materialization” measures here:** `GameMoveDeriver` + `GamePositionSummaryFactory` + orchestration in `AnalyticsMaterializationService`, counting **synthetic rows** that would be written each pass: **2** `GamePositionSummary` rows + **1** `GameMoveFact` per “game” (initial ply −1 and ply 0 after 1.e4 on an otherwise empty board fixture).

## In-memory harness (no database I/O)

Uses `MaterializationWriteOnlyNoOpRepository` so `ReplaceGameMovesForGame` / `ReplaceGamePositionSummariesForGame` do not touch SQL.

From the repo root:

```bash
dotnet run --project src/Analyser -- --profile-materialization
```

Optional: `--iterations N` (default **5000**). The process prints **games/s** and **(summary + move) rows/s**.

### Example result line

Interpretation: higher **games/s** and **rows/s** mean faster CPU-only derivation for this fixed two-ply fixture on your machine. Record the line in your notes or in a PR when you change deriver/summary logic.

| Machine / profile (fill in) | Iterations | Wall (ms) | Games/s | Rows/s |
|-----------------------------|------------|-----------|---------|--------|
| *(local dev, example)*     | 5000       | *(paste from console)* | *(paste)* | *(paste)* |

## End-to-end note (includes SQL)

For **DB-bound** throughput (hydrate + replace), use **`--backfill-analytics`** with a capped batch and measure wall time in your shell, for example:

```powershell
Measure-Command { dotnet run --project src/Analyser -- --backfill-analytics --max-games 100 }
```

Throughput then depends on network, SQL Server, game length, and concurrent load — record **games processed / elapsed** separately from the in-memory harness above.

## Tests

`MaterializationPerfHarnessTests` runs a **small** iteration count to guard regressions (harness completes; timing is not asserted in CI).
