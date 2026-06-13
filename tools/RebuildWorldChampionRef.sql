-- Rebuild world-champion reference data from scratch (local recovery).
--
-- This script is SELF-CONTAINED. It does not rely on DbUp re-running migration 013.
-- DbUp records each script once in schemaversions; editing 013 later has no effect
-- on a database that already ran the old version.
--
-- Preserves:
--   - All dbo.Player rows (names/ids)
--   - Ref.FidePlayer catalog
--   - schemaversions entries 001-012
--
-- After this script:
--   dotnet run --project src/Migrations/Migrations.csproj
--   (only runs player metadata enrichment; 013-017 are marked applied)

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE Chess;
GO

PRINT 'Step 1: Clear player metadata...';
UPDATE dbo.Player
SET WasWorldChampion = 0,
    FideId = NULL,
    Federation = NULL,
    Sex = NULL,
    FideTitle = NULL,
    BirthYear = NULL;

IF COL_LENGTH(N'dbo.Player', N'WorldChampionId') IS NOT NULL
    UPDATE dbo.Player SET WorldChampionId = NULL;
GO

PRINT 'Step 2: Drop dbo.Player world-champion link...';
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Player_WorldChampion')
    ALTER TABLE dbo.Player DROP CONSTRAINT FK_Player_WorldChampion;
GO

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Player_WorldChampionId' AND object_id = OBJECT_ID(N'dbo.Player'))
    DROP INDEX IX_Player_WorldChampionId ON dbo.Player;
GO

IF COL_LENGTH(N'dbo.Player', N'WorldChampionId') IS NOT NULL
    ALTER TABLE dbo.Player DROP COLUMN WorldChampionId;
GO

PRINT 'Step 3: Drop world-champion tables...';
IF OBJECT_ID(N'Ref.WorldChampionAlias', N'U') IS NOT NULL
    DROP TABLE Ref.WorldChampionAlias;
GO

IF OBJECT_ID(N'Ref.WorldChampion', N'U') IS NOT NULL
    DROP TABLE Ref.WorldChampion;
GO

IF OBJECT_ID(N'Ref.WorldChampion', N'U') IS NOT NULL
BEGIN
    RAISERROR('Failed to drop Ref.WorldChampion.', 16, 1);
    RETURN;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Ref')
    EXEC(N'CREATE SCHEMA Ref');
GO

PRINT 'Step 4: Create Ref.WorldChampion (all constraints)...';
CREATE TABLE Ref.WorldChampion (
    Id              INT            IDENTITY(1, 1) NOT NULL,
    Surname         NVARCHAR(200)  NOT NULL,
    Forenames       NVARCHAR(400)  NOT NULL,
    ChampionOrder   SMALLINT       NOT NULL,
    ReignStartYear  SMALLINT       NULL,
    ReignEndYear    SMALLINT       NULL,
    CONSTRAINT PK_Ref_WorldChampion PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Ref_WorldChampion_ChampionOrder UNIQUE (ChampionOrder),
    CONSTRAINT UQ_Ref_WorldChampion_Surname_Forenames UNIQUE (Surname, Forenames),
    CONSTRAINT CK_Ref_WorldChampion_Id_ChampionOrder CHECK (Id = ChampionOrder)
);
GO

PRINT 'Step 5: Create Ref.WorldChampionAlias...';
CREATE TABLE Ref.WorldChampionAlias (
    Id              INT            IDENTITY(1, 1) NOT NULL,
    WorldChampionId INT            NOT NULL,
    Surname         NVARCHAR(200)  NOT NULL,
    Forenames       NVARCHAR(400)  NOT NULL,
    CONSTRAINT PK_Ref_WorldChampionAlias PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Ref_WorldChampionAlias_Champion
        FOREIGN KEY (WorldChampionId) REFERENCES Ref.WorldChampion (Id),
    CONSTRAINT UQ_Ref_WorldChampionAlias_Surname_Forenames UNIQUE (Surname, Forenames)
);
GO

PRINT 'Step 6: Seed champions (Id = ChampionOrder = 1..18)...';
SET IDENTITY_INSERT Ref.WorldChampion ON;

INSERT INTO Ref.WorldChampion (Id, Surname, Forenames, ChampionOrder, ReignStartYear, ReignEndYear)
VALUES
    ( 1, N'Steinitz',   N'Wilhelm',       1,  1886, 1894),
    ( 2, N'Lasker',     N'Emanuel',       2,  1894, 1921),
    ( 3, N'Capablanca', N'Jose Raul',     3,  1921, 1927),
    ( 4, N'Alekhine',   N'Alexander',     4,  1927, 1946),
    ( 5, N'Euwe',       N'Max',           5,  1935, 1937),
    ( 6, N'Botvinnik',  N'Mikhail',       6,  1948, 1963),
    ( 7, N'Smyslov',    N'Vasily',        7,  1957, 1958),
    ( 8, N'Tal',        N'Mikhail',       8,  1960, 1961),
    ( 9, N'Petrosian',  N'Tigran',        9,  1963, 1969),
    (10, N'Spassky',    N'Boris',        10,  1969, 1972),
    (11, N'Fischer',    N'Bobby',        11,  1972, 1975),
    (12, N'Karpov',     N'Anatoly',      12,  1975, 1985),
    (13, N'Kasparov',   N'Garry',        13,  1985, 2000),
    (14, N'Kramnik',    N'Vladimir',     14,  2000, 2007),
    (15, N'Anand',      N'Viswanathan',  15,  2007, 2013),
    (16, N'Carlsen',    N'Magnus',       16,  2013, 2023),
    (17, N'Ding',       N'Liren',        17,  2023, 2024),
    (18, N'Gukesh',     N'Dommaraju',    18,  2024, NULL);

SET IDENTITY_INSERT Ref.WorldChampion OFF;
GO

INSERT INTO Ref.WorldChampionAlias (WorldChampionId, Surname, Forenames)
VALUES
    ( 1, N'Steinitz',   N'William'),
    ( 2, N'Lasker',     N'Emmanuel'),
    ( 3, N'Capablanca', N'José Raúl'),
    ( 4, N'Alekhine',   N'Alexandre'),
    (11, N'Fischer',    N'Robert James');
GO

PRINT 'Step 7: Add dbo.Player.WorldChampionId...';
ALTER TABLE dbo.Player ADD WorldChampionId INT NULL;
GO

ALTER TABLE dbo.Player
    ADD CONSTRAINT FK_Player_WorldChampion
    FOREIGN KEY (WorldChampionId) REFERENCES Ref.WorldChampion (Id);
GO

CREATE NONCLUSTERED INDEX IX_Player_WorldChampionId ON dbo.Player (WorldChampionId)
WHERE WorldChampionId IS NOT NULL;
GO

PRINT 'Step 8: Verify constraints...';
DECLARE @missing NVARCHAR(4000) = NULL;

IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE name = N'PK_Ref_WorldChampion'
      AND parent_object_id = OBJECT_ID(N'Ref.WorldChampion')
      AND type = N'PK')
    SET @missing = ISNULL(@missing + N', ', N'') + N'PK_Ref_WorldChampion';

IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE name = N'UQ_Ref_WorldChampion_Surname_Forenames'
      AND parent_object_id = OBJECT_ID(N'Ref.WorldChampion')
      AND type = N'UQ')
    SET @missing = ISNULL(@missing + N', ', N'') + N'UQ_Ref_WorldChampion_Surname_Forenames';

IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE name = N'UQ_Ref_WorldChampion_ChampionOrder'
      AND parent_object_id = OBJECT_ID(N'Ref.WorldChampion')
      AND type = N'UQ')
    SET @missing = ISNULL(@missing + N', ', N'') + N'UQ_Ref_WorldChampion_ChampionOrder';

IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE name = N'CK_Ref_WorldChampion_Id_ChampionOrder'
      AND parent_object_id = OBJECT_ID(N'Ref.WorldChampion')
      AND type = N'C')
    SET @missing = ISNULL(@missing + N', ', N'') + N'CK_Ref_WorldChampion_Id_ChampionOrder';

IF (SELECT COUNT(*) FROM Ref.WorldChampion) <> 18
    SET @missing = ISNULL(@missing + N', ', N'') + N'18 champion rows';

IF @missing IS NOT NULL
BEGIN
    RAISERROR('VERIFY FAILED: %s', 16, 1, @missing);
    RETURN;
END
GO

PRINT 'Step 9: Update schemaversions (mark 013-017 as applied)...';
DELETE FROM dbo.schemaversions
WHERE scriptname IN (
    N'013_CreateRefWorldChampion.sql',
    N'014_CreateRefFidePlayer.sql',
    N'015_SeedRefFidePlayer.sql',
    N'016_AddPlayerWorldChampionId.sql',
    N'017_BackfillPlayerWorldChampionLinks.sql',
    N'017_SeedRefWorldChampion.sql',
    N'018_CreateRefWorldChampionAlias.sql',
    N'019_SeedRefWorldChampion.sql'
);
GO

INSERT INTO dbo.schemaversions (scriptname, applied)
VALUES
    (N'013_CreateRefWorldChampion.sql', GETUTCDATE()),
    (N'014_CreateRefFidePlayer.sql', GETUTCDATE()),
    (N'015_SeedRefFidePlayer.sql', GETUTCDATE()),
    (N'016_AddPlayerWorldChampionId.sql', GETUTCDATE()),
    (N'017_SeedRefWorldChampion.sql', GETUTCDATE());
GO

PRINT 'Rebuild complete. Constraints on Ref.WorldChampion:';
SELECT name, type_desc
FROM sys.objects
WHERE parent_object_id = OBJECT_ID(N'Ref.WorldChampion')
  AND type IN ('C', 'UQ', 'PK')
ORDER BY name;
GO

PRINT 'Next: dotnet run --project src/Migrations/Migrations.csproj  (enrichment only)';
GO
