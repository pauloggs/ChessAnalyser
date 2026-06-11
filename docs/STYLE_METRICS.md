# ChessAnalyser — playing-style metrics (research)

**Location:** `docs/` alongside [DESIGN.md](./DESIGN.md), [PLAN.md](./PLAN.md), and [EXAMPLE_ANALYSES.md](./EXAMPLE_ANALYSES.md).  
**Purpose:** Capture how chess playing style is assessed in the literature and tools, what ChessAnalyser can measure **without an engine**, and how individual metrics combine into interpretable profiles.  
**Implementation backlog:** [PLAN.md §12.7](./PLAN.md) (corpus benchmarks), then [PLAN.md §12.6](./PLAN.md) Phase 3+.

---

## 1. Why style is hard to measure

Playing style is not a single scalar. Analysts and researchers use **many behavioural fingerprints** and compare players in profile space. A metric that works in isolation (e.g. capture rate) is often confounded by **opening choice**, **opponent strength**, and **era**. Style metrics should therefore:

- Be computed **per player** with explicit `playerSurname` / `playerForenames` and optional `playerColour`.
- Prefer **middlegame windows** (e.g. plies 15–40) when opening noise should be reduced.
- Require **sufficient sample size** (dozens of games; literature often excludes short draws and out-of-prime years).
- Be read as **choices**, not **quality** — without engine evaluation, high capture rate does not distinguish Tal from a blunderer.
- Prefer **corpus-relative** readings (percentile vs your loaded database) over isolated scalars — see §8 and [DESIGN.md §12](./DESIGN.md).

---

## 2. External references (how others assess style)

| Source | Method | Dimensions relevant to ChessAnalyser |
|--------|--------|--------------------------------------|
| [ChessBase Style Report](https://help.chessbase.com/CBase/18/Eng/criteria_of_the_style_report.htm) | Fast heuristics, minimal engine | Theory/repertoire, fighting spirit, aggressiveness, risk, positional play, endgame skill |
| [jk_182 — Identifying the Style of Different Players](https://lichess.org/@/jk_182/blog/identifying-the-style-of-different-players/WnfPnBli) | 43 move-derived variables, PCA (no engine) | Pawn vs piece moves, centre play, king attacks, checks, material imbalance, material on board |
| [Chessiverse personality test](https://chessiverse.com/chess-personality) | ~51 metrics, normalized vs population | Opening breadth, middlegame complexity, capture timing, endgame symmetry vs asymmetry |
| [Data-Driven Chess Typology](https://www.diva-portal.org/smash/get/diva2:1977201/FULLTEXT01.pdf) | Feature engineering + ML (Activist / Theorist / Reflector / Pragmatist) | Phase distribution, castling timing, material volatility, mobility, king safety, sacrifices |
| [Larry Kaufman — material imbalances](https://www.chess.com/article/view/the-evaluation-of-material-imbalances-by-im-larry-kaufman) | Engine-tested piece values | Bishop pair (~½ pawn), bishop ≈ knight when unpaired — motivates bishop-pair retention metrics |

**ChessAnalyser alignment:** The project deliberately avoids engine evaluation on the hot path (see PLAN §12.4). Style metrics here measure **move and position patterns** from `GameMove`, `GamePositionSummary`, and `Game` — the same class of signals ChessBase and jk_182 use before bringing in ACPL or sharpness scores.

---

## 3. Style dimensions and data sources

| Dimension | What it captures | Primary tables in ChessAnalyser |
|-----------|------------------|--------------------------------|
| **King safety / tempo** | When and how the king is sheltered | `GameMove` (castling flags, king moves) |
| **Complexity vs simplicity** | Material on board, volatility of balance | `GamePositionSummary` |
| **Piece philosophy** | Bishops vs knights, bishop pair | `GamePositionSummary` |
| **Queen behaviour** | Early queen activity, queen trades | `GameMove`, `GamePositionSummary` |
| **Spatial aggression** | Centre and forward play | `GameMove` (`ToSquare`) |
| **Exchange temperament** | Captures, promotions | `GameMove` |
| **Game shape** | Length, draws, decisiveness | `Game`, ply counts from summaries |
| **Repertoire** | Opening breadth and concentration | `Game.Eco` |

**Not available without schema or engine extensions:** checks (`IsCheck`), legal-move mobility, pawn-structure motifs (isolated/doubled/passers), sacrifice detection, ACPL, position sharpness/WDL.

---

## 4. Metric catalogue (planned)

Full implementation specs live in [PLAN.md §12.6](./PLAN.md). Summary by phase:

### Phase 1 — highest signal / lowest effort (implemented)

| Key | Style signal |
|-----|----------------|
| `AverageCastlingPly` | Early castle ≈ pragmatic/safe; late or never ≈ riskier |
| `AverageMaterialVolatility` | Std dev of material balance across a game — dynamic vs stable play |

### Phase 2 — piece philosophy (implemented)

| Key | Style signal |
|-----|----------------|
| `BishopPairFrequency` | Retaining both bishops — positional / long-diagonal preference |
| `MinorPieceComposition` | Avg bishops − knights on player's side in a ply window (default plies 15–30) |

### Phase 3 — exchange temperament

| Key | Style signal |
|-----|----------------|
| `CaptureRate` | Captures per move — tactical exchanger |
| `QueenTradeRate` | Queens exchanged before a ply threshold — simplifier vs attacker |

### Phase 4 — spatial aggression

| Key | Style signal |
|-----|----------------|
| `CentreMoveRate` | Moves to central squares |
| `ForwardMoveRate` | Moves landing in opponent's half |

### Phase 5 — king safety extensions

| Key | Style signal |
|-----|----------------|
| `CastlingSidePreference` | Kingside vs queenside share |
| `OppositeSideCastlingRate` | Both players castle to different wings |
| `UncastledKingRate` | Games where player never castles |

### Phase 6 — queen timing

| Key | Style signal |
|-----|----------------|
| `FirstQueenMovePly` | Normalized ply of first queen move |

### Phase 7 — game shape

| Key | Style signal |
|-----|----------------|
| `AverageGameLength` | Mean plies per game |
| `ShortDrawRate` | Short draws as share of all draws (fighting spirit proxy) |

### Phase 8 — repertoire

| Key | Style signal |
|-----|----------------|
| `EcoDiversity` | Distinct ECO codes per player |
| `EcoConcentration` | Share of games in top-N ECO families |

### Phase 9 — materialization extensions (later)

| Extension | Enables |
|-----------|---------|
| `IsCheck` on `GameMove` | Check frequency |
| `Phase` on `GamePositionSummary` | Opening/middlegame/endgame share |
| `PlayerStyleProfile` composite metric | Normalized vector for comparison / future PCA |

---

## 5. Style profiles (metric combinations)

Single metrics are weak; literature consistently combines them. Example **exploratory profiles** (run several metrics for the same player filter and compare):

**Aggression / activist**

- Low `AverageCastlingPly` or high `OppositeSideCastlingRate`
- High `ForwardMoveRate`, `CentreMoveRate`, `CaptureRate`
- Low `QueenTradeRate`
- High `MaterialVolatility`

**Positional / reflector**

- High `BishopPairFrequency`
- Moderate `PawnMoveRatio` (future) with long `AverageGameLength`
- Low `MaterialVolatility`
- Late `FirstQueenMovePly`

**Simplifier / technical**

- Low average material at midgame (existing `AverageMaterialByPlayerAtMove`)
- High `QueenTradeRate`, high `CaptureRate`
- Long games, high promotion rate (future)

**Risk-taker**

- High `MaterialVolatility`, high `UncastledKingRate`
- Asymmetric minor-piece endgames (future `BishopVsKnightEndgameRate`)
- Sharp ECO concentration (future curated sharp-opening list)

A future **`PlayerStyleProfile`** metric could return a small normalized vector; v1 is **multiple registered executors** compared manually or in the UI.

---

## 6. Interpretation caveats

1. **Openings dominate early plies** — filter by ply window or exclude opening plies when comparing middlegame style.
2. **Opponent and era** — strong opponents and modern preparation homogenize choices.
3. **Sample size** — prefer ≥ 30–50 games per player for stable averages.
4. **Correlation ≠ causation** — high capture rate may reflect sharp openings, not innate aggression.
5. **No engine** — measures preferences and patterns, not objective soundness.

---

## 7. Validation approach

When implementing a new style metric, sanity-check against players with well-known reputations (e.g. Tal vs Petrosian, Kasparov vs Karpov) using the same year and colour filters. Direction of difference matters more than absolute numbers.

---

## 8. Corpus benchmarks (interpretation)

Raw values like `AverageMaterialVolatility = 1.34` are **not self-explanatory**. ChessAnalyser is
adding **corpus-relative benchmarks** ([DESIGN.md §12](./DESIGN.md), [PLAN.md §12.7](./PLAN.md)):

| Column | Meaning |
|--------|---------|
| `CorpusAverage` | Mean of the same metric across other eligible players in your DB (same filters) |
| `DeltaFromCorpus` | Subject minus corpus average |
| `CorpusPercentile` | Where the subject ranks 0–100 among eligible players (higher = higher metric value) |
| `CorpusEligiblePlayerCount` | How many players met the minimum game threshold |

**How to read a row**

> Fischer: volatility 1.34, corpus avg 1.08, delta +0.26, 78th percentile  
> → More material swing than most players **in this database** under the chosen filters.

**Caveats**

- Benchmarks are **corpus-local**, not universal chess truth.
- Requires enough players and games (`benchmarkMinGames`, default 30).
- Subject is **excluded** from the corpus mean so large game counts do not dominate the baseline.
- Opt in with `includeCorpusBenchmark: true` on the metric query until defaults change.

**Metrics with corpus benchmarks (v1):** `AverageMaterialVolatility`, `AverageCastlingPly`,
`BishopPairFrequency`, `MinorPieceComposition`, `CaptureRate`. (`QueenTradeRate` — follow-up PR.)

Phase 3+ style metrics should ship with benchmark support where possible so new scalars are not
published without context.

---

*Update this file when literature review or metric definitions change. Tick implementation items in [PLAN.md §12.6](./PLAN.md) and [PLAN.md §12.7](./PLAN.md).*
