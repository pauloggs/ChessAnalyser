# ChessAnalyser — database backup and transfer

**Purpose:** Move a populated **`Chess`** SQL Server database from one machine to another without re-parsing PGN files. Parsing and materialization are slow; the database is the valuable asset.

**Applies to:** SQL Server as configured via `ConnectionStrings:ChessConnection` in `src/Migrations/appsettings.json` and `src/Analyser/appsettings.json` — either **Docker** (Linux container, port `1433` on `127.0.0.1`) or **native Windows** SQL Server (Express, Developer, or full edition).

---

## 1. What to move

| Item | Required? | Notes |
|------|-----------|--------|
| **SQL Server database `Chess`** | **Yes** | Games, `BoardPosition`, `GameMove`, `GamePositionSummary`, players, FIDE catalog (`Ref.FidePlayer`), migration history (`dbo.schemaversions`) |
| **Repo + connection strings** | **Yes** | Clone on the new machine; point both apps at the restored database |
| **PGN source files** | No | Only needed if you plan to **load additional** games later |
| **`data/fide/players_list_foa.txt`** | No | Not needed if `Ref.FidePlayer` is already populated in the restored database |

DbUp records applied migration scripts in **`dbo.schemaversions`**. A restored database already knows its schema. Running migrations on the new machine is safe: only **new** scripts (not yet in `schemaversions`) will execute.

---

## 2. Where is the database stored?

The app does **not** store data in the repo. It connects to a **SQL Server instance**; the logical database name is **`Chess`**.

Example connection string:

```json
"ChessConnection": "Server=127.0.0.1;Database=Chess;TrustServerCertificate=True;User Id=SA;Password=YourPassword"
```

- **`Server=127.0.0.1`** — the SQL Server *instance* (host + port), not a file path.
- **`Database=Chess`** — the database *name* on that instance.

To confirm the server is reachable and list physical file locations:

```cmd
sqlcmd -S 127.0.0.1 -U SA -P "YourPassword" -C -Q "SELECT d.name AS DatabaseName, f.type_desc AS FileType, f.physical_name AS PhysicalPath FROM sys.master_files f INNER JOIN sys.databases d ON f.database_id = d.database_id WHERE d.name = N'Chess'"
```

### 2.1 Docker SQL Server (common local setup)

If SQL Server runs in Docker (e.g. container `sql1`, image `mcr.microsoft.com/mssql/server`, port `1433` published to the host):

| What | Where |
|------|--------|
| **How the app reaches it** | `127.0.0.1:1433` — same `sqlcmd` / connection string as native SQL Server |
| **Data files** | **Inside the container**, typically `/var/opt/mssql/data/Chess.mdf` and `Chess_log.ldf` |
| **Windows Explorer** | You will **not** see `.mdf` files under your user folder; they are not on the Windows filesystem unless you mounted a volume |

List the running container:

```cmd
docker ps
```

**Important:** `BACKUP DATABASE … TO DISK = 'C:\Backups\…'` uses a path **inside the Linux container**, not `C:\` on Windows. Back up to a container path (e.g. `/var/opt/mssql/data/Chess.bak`), then copy the file out with `docker cp`. See [§3](#3-docker-sql-server--backup-and-restore).

### 2.2 Native Windows SQL Server

If SQL Server is installed directly on Windows (no Docker), data files usually live under:

```text
C:\Program Files\Microsoft SQL Server\MSSQL<ver>.<INSTANCE>\MSSQL\DATA\Chess.mdf
```

`BACKUP … TO DISK = 'C:\Backups\Chess.bak'` then refers to a normal Windows path. See [§4](#4-native-windows-sql-server--backup-and-restore).

---

## 3. Docker SQL Server — backup and restore

Use this section when `sqlcmd -S 127.0.0.1` works and `docker ps` shows an `mssql/server` container. Adjust container name (`sql1`) and passwords to match your setup.

### 3.1 Source machine — backup

1. **Stop writes** — stop the Analyser app and any ETL/load in progress.

2. **Back up inside the container** (path is on the container filesystem — **not** `C:\` on Windows):

```cmd
sqlcmd -S 127.0.0.1 -U SA -P "YourPassword" -C -Q "BACKUP DATABASE Chess TO DISK = N'/var/opt/mssql/data/Chess.bak' WITH COMPRESSION, INIT, STATS = 10"
```

   Do **not** use `C:\Backups\Chess.bak` here when SQL Server runs in Docker. That path is interpreted inside the Linux container and typically fails with:

   ```text
   Msg 3201 … Cannot open backup device 'C:\Backups\Chess.bak'. Operating system error 5 (Access is denied.)
   ```

3. **Copy the `.bak` to Windows** — run as **separate** commands (do not paste on one line):

**Command Prompt (cmd):**

```cmd
mkdir C:\Backups
docker cp sql1:/var/opt/mssql/data/Chess.bak C:\Backups\Chess.bak
dir C:\Backups\Chess.bak
```

Or one line in cmd (creates folder if missing):

```cmd
mkdir C:\Backups 2>nul && docker cp sql1:/var/opt/mssql/data/Chess.bak C:\Backups\Chess.bak
```

**PowerShell:**

```powershell
New-Item -ItemType Directory -Path C:\Backups -Force | Out-Null
docker cp sql1:/var/opt/mssql/data/Chess.bak C:\Backups\Chess.bak
Get-Item C:\Backups\Chess.bak
```

   Expect a compressed `.bak` on the order of **~100–200 MB** for a medium corpus (size varies with data).

4. **Transfer** `C:\Backups\Chess.bak` to the new machine (USB, network share, etc.).

Optional — remove the backup file inside the container to free space:

```cmd
docker exec sql1 rm /var/opt/mssql/data/Chess.bak
```

### 3.2 New machine — restore

1. **Prerequisites:** Docker, SQL Server container running on port `1433`, .NET SDKs, cloned repo — see [§5](#5-after-restore--run-the-app).

   Start a container if needed (example; set `ACCEPT_EULA` and `MSSQL_SA_PASSWORD`):

```powershell
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourNewSaPassword" -p 1433:1433 --name sql1 -d mcr.microsoft.com/mssql/server:2022-latest
```

   Prefer **same or newer** SQL Server image version than the source.

2. **Copy the backup into the container:**

```cmd
docker cp C:\Backups\Chess.bak sql1:/var/opt/mssql/data/Chess.bak
```

3. **Restore** (path is inside the container):

```cmd
sqlcmd -S 127.0.0.1 -U SA -P "YourNewSaPassword" -C -Q "RESTORE DATABASE Chess FROM DISK = N'/var/opt/mssql/data/Chess.bak' WITH REPLACE, STATS = 10"
```

4. Continue with [§5](#5-after-restore--run-the-app) (connection strings, migrations, run app).

### 3.3 Docker — SSMS

You can connect SSMS to **`127.0.0.1,1433`** with SQL authentication (`SA` / your password). For backup/restore destinations in SSMS, paths are still **inside the container** unless you use a bind-mounted backup folder. The `sqlcmd` + `docker cp` flow above is usually simpler.

### 3.4 Docker — optional bind mount for backups

To write backups directly to a Windows folder, recreate the container with a mount (example):

```powershell
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourPassword" -p 1433:1433 --name sql1 -v C:\Backups:/var/opt/mssql/backup -d mcr.microsoft.com/mssql/server:2022-latest
```

Then:

```powershell
sqlcmd -S 127.0.0.1 -U SA -P "YourPassword" -C -Q "BACKUP DATABASE Chess TO DISK = N'/var/opt/mssql/backup/Chess.bak' WITH COMPRESSION, INIT, STATS = 10"
```

`Chess.bak` appears immediately under `C:\Backups` on the host. **Warning:** recreating the container without preserving `/var/opt/mssql/data` loses data unless you have a backup or a data volume — plan mounts before switching.

---

## 4. Native Windows SQL Server — backup and restore

Use this section when SQL Server is installed on Windows (not Docker).

### 4.1 Source machine — backup

1. **Stop writes** — stop the Analyser app and any ETL/load in progress.

2. **Back up:**

```powershell
sqlcmd -S 127.0.0.1 -U SA -P "YourPassword" -C -Q "BACKUP DATABASE Chess TO DISK = N'C:\Backups\Chess.bak' WITH COMPRESSION, INIT, STATS = 10"
```

Create `C:\Backups` first if it does not exist. **`COMPRESSION`** reduces file size for large corpora.

**SSMS:** Databases → **Chess** → Tasks → **Back Up…** → destination `C:\Backups\Chess.bak`.

3. **Copy** `Chess.bak` to the new machine.

### 4.2 New machine — restore

```powershell
sqlcmd -S 127.0.0.1 -U SA -P "YourNewSaPassword" -C -Q "RESTORE DATABASE Chess FROM DISK = N'C:\Backups\Chess.bak' WITH REPLACE, STATS = 10"
```

**SSMS:** Databases → **Restore Database…** → Device → select `Chess.bak`; adjust file paths on the **Files** tab if needed.

If restore fails on file paths, inspect the backup:

```powershell
sqlcmd -S 127.0.0.1 -U SA -P "YourNewSaPassword" -C -Q "RESTORE FILELISTONLY FROM DISK = N'C:\Backups\Chess.bak'"
```

Then restore with explicit **`MOVE`** clauses for each logical name.

Continue with [§5](#5-after-restore--run-the-app).

---

## 5. After restore — run the app

### 5.1 Prerequisites

| Component | Notes |
|-----------|--------|
| **SQL Server** | Docker container or native install; database **`Chess`** restored |
| **.NET SDK** | **8.0** and **9.0** bands — see [README.md](../README.md) |
| **Repository** | `git clone`, then `dotnet restore ChessAnalyser.sln` |

### 5.2 Configure connection strings

Set the same database name (**`Chess`**) in:

- `src/Migrations/appsettings.json`
- `src/Analyser/appsettings.json`

Example:

```json
"ConnectionStrings": {
  "ChessConnection": "Server=127.0.0.1;Database=Chess;TrustServerCertificate=True;User Id=SA;Password=YourNewSaPassword"
}
```

Override without editing files (PowerShell):

```powershell
$env:ConnectionStrings__ChessConnection = "Server=127.0.0.1;Database=Chess;TrustServerCertificate=True;User Id=SA;Password=YourNewSaPassword"
```

The **SA password** may differ on the new machine; only the connection string must match your local setup.

### 5.3 Optional — run migrations

From the repo root:

```bash
dotnet run --project src/Migrations/Migrations.csproj
```

On a fully restored database this should report that existing scripts are already applied. If you have pulled newer code since the backup, only **new** migration scripts will run.

### 5.4 Run the app

```bash
dotnet run --project src/Analyser/Analyser.csproj
```

### 5.5 Sanity checks

```sql
SELECT COUNT(*) AS GameCount FROM dbo.Game;
SELECT COUNT(*) AS PositionCount FROM dbo.BoardPosition;
SELECT COUNT(*) AS FideCatalogCount FROM Ref.FidePlayer;
```

---

## 6. When you still need other steps

| Situation | Action |
|-----------|--------|
| **Add new PGN files** | Run the normal load/ETL on the new machine; existing games are skipped by processed-game logic. |
| **Empty `Ref.FidePlayer` in backup** | Place `data/fide/players_list_foa.txt` locally and run migrations — see [data/fide/README.md](../data/fide/README.md). |
| **Games with positions but no analytics rows** | `dotnet run --project src/Analyser -- --backfill-analytics` — see [src/Migrations/README.md](../src/Migrations/README.md). |
| **Schema history / CI** | Unrelated to day-to-day app use; only needed when changing migrations in development. |

You do **not** need to re-parse PGNs solely to use the app on the new machine if the restored database already contains your games and positions.

---

## 7. Troubleshooting

| Problem | Likely cause | What to try |
|---------|--------------|-------------|
| `Msg 3201` / Access denied on `C:\Backups\…` (Docker) | `BACKUP` path is inside Linux container, not Windows | Use `/var/opt/mssql/data/Chess.bak`, then `docker cp` to `C:\Backups` |
| `The syntax of the command is incorrect` (cmd) | Two commands pasted as one line, or PowerShell-only flags (`-Force`) in cmd | Run `mkdir` and `docker cp` separately; in cmd use `mkdir C:\Backups` not `mkdir -Force` |
| `sqlcmd` cannot connect | Container stopped, wrong password, or port not published | `docker ps`; verify `1433:1433`; test with `-C` (trust server certificate) |
| Backup path not found (Docker) | Used `C:\...` in `BACKUP` — that path is inside Linux container | Use `/var/opt/mssql/data/...` then `docker cp` |
| `.bak` not visible on Windows | File still inside container | `docker cp sql1:/var/opt/mssql/data/Chess.bak C:\Backups\` |
| Data lost after recreating container | No data volume; data was only in container filesystem | Restore from `.bak`; consider a named Docker volume for `/var/opt/mssql/data` |
| Restore fails with version error | Target SQL Server older than source | Use same or newer image / edition on the new machine |
| Restore fails on file path (native) | Different default data directory | SSMS **Files** tab or `RESTORE … WITH MOVE` |
| App cannot connect | Wrong server, password, or firewall | Match `ChessConnection`; ensure SQL Server accepts TCP on `127.0.0.1` |
| Migrations re-run old scripts | Unlikely if `schemaversions` restored | Verify `SELECT * FROM dbo.schemaversions` is populated |
| Missing player FIDE metadata | Catalog empty in backup | Add FIDE file and run migrations host once |

---

## 8. Alternative — BACPAC export

For portability (e.g. Azure SQL or strict version boundaries), export/import a **BACPAC** via SSMS or `SqlPackage.exe`. For typical local moves, a **`.bak` backup/restore** is simpler. With Docker, BACPAC paths have the same inside-container vs `docker cp` considerations as `.bak` files.

---

*Related: [README.md](../README.md) (quick start), [src/Migrations/README.md](../src/Migrations/README.md) (schema and tooling).*
