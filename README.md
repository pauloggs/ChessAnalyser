# ChessAnalyser

.NET application that ingests chess **PGN** files, rebuilds **bitboard** positions for every half-move, resolves **players**, and persists games and positions to **SQL Server**. A **web API** (`Analyser`) exposes load and inspection workflows; batch ETL can run game-by-game to limit memory use.

---

## Features

- **PGN parsing** — Games, headers (tags), SAN plies, and error capture for bad or incomplete games.
- **Board replay** — Derives `BoardPosition` snapshots per ply (including the initial position at `PlyIndex -1`).
- **Persistence** — `Game`, `BoardPosition`, `Player`, and parse-error tables via Dapper.
- **Planned (see docs)** — Board-position **analytics** framework: derived **move** facts, rollups, and internal metrics APIs, specified in [DESIGN.md](./DESIGN.md) and [PLAN.md](./PLAN.md) (implementation checklist not yet completed).

---

## Requirements

| Component | Version / notes |
|-----------|------------------|
| **.NET SDK** | **8.0** and **9.0** bands (e.g. `Services` / `Repositories` target net8.0; `Analyser` targets net9.0). Install SDKs that satisfy both. |
| **SQL Server** | Database must exist; migrations do not create the server or database. |
| **OS** | Windows-friendly paths in docs; core code is standard .NET. |

---

## Quick start

### 1. Clone and restore

```bash
git clone <repository-url>
cd ChessAnalyser
dotnet restore ChessAnalyser.sln
```

### 2. Configure the database

- **Migrations app:** set `ConnectionStrings:ChessConnection` in `src/Migrations/appsettings.json`, or override with the environment variable `ConnectionStrings__ChessConnection`.
- **Analyser / API:** use the same logical database; configure connection strings in that project’s settings (e.g. `appsettings.json` / user secrets) as used by `ChessRepository`.

### 3. Apply schema (DbUp)

From the repo root:

```bash
dotnet run --project src/Migrations/Migrations.csproj
```

Details and script order: [src/Migrations/README.md](src/Migrations/README.md).

### 4. Run the web API

```bash
dotnet run --project src/Analyser/Analyser.csproj
```

Swagger is available when enabled in development (Swashbuckle).

### 5. Tests

```bash
dotnet test ChessAnalyser.sln
```

---

## Solution layout

| Path | Role |
|------|------|
| `src/Analyser/` | ASP.NET Core host, controllers, Swagger. |
| `src/Services/` | PGN parsing, ETL, board/move logic, persistence orchestration. |
| `src/Repositories/` | SQL access (Dapper), `SqlStatements`. |
| `src/Interfaces/` | DTOs and shared types (`Game`, `BoardPosition`, …). |
| `src/Migrations/` | DbUp console — SQL scripts under `Scripts/`. |
| `test/` | Unit and integration-style tests (`ServicesTests`, `RepositoriesTests`, `AnalyserTests`, …). |

---

## Documentation

| Document | Purpose |
|----------|---------|
| [DESIGN.md](./DESIGN.md) | Requirements and locked decisions for **board-position analytics** (facts, dimensions, C# vs SQL, rollups). |
| [PLAN.md](./PLAN.md) | Implementation plan, migrations outline, and **§11 checklist** for analytics work. |
| [docs/HANDOVER.md](docs/HANDOVER.md) | Agent/human handover: where analytics implementation left off. |
| [docs/](docs/) | Additional notes (coverage gaps, audits, reviews). |

**Process:** larger features follow **Design → Plan → Implement (DPI)** with most effort on design and plan. A personal Cursor skill `dpi-workflow` can enforce the same habit across repos (`~/.cursor/skills/dpi-workflow/`).

---

## Contributing / development notes

- Prefer **parameterized** SQL (existing pattern in `Repositories`).
- New database changes: add numbered scripts under `src/Migrations/Scripts/` and document them in `src/Migrations/README.md`.
- For analytics work, align with [PLAN.md](./PLAN.md) before changing schema or ETL in ways that conflict with [DESIGN.md](./DESIGN.md).

---

## License

No license file is present in this repository; add one if the project is published or shared.
