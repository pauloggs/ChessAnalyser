-- Idempotent: PGN header / analytics dimension columns on dbo.Game (DESIGN §3.5, PLAN §4.1).
-- GameYear NULL = omit game from year-based metrics; DateTag stores raw [Date] for audit.

IF OBJECT_ID(N'dbo.Game', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Game') AND name = N'Event')
        ALTER TABLE dbo.Game ADD Event NVARCHAR(500) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Game') AND name = N'Site')
        ALTER TABLE dbo.Game ADD Site NVARCHAR(500) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Game') AND name = N'DateTag')
        ALTER TABLE dbo.Game ADD DateTag NVARCHAR(32) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Game') AND name = N'GameYear')
        ALTER TABLE dbo.Game ADD GameYear SMALLINT NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Game') AND name = N'Eco')
        ALTER TABLE dbo.Game ADD Eco NVARCHAR(16) NULL;
END
GO

IF OBJECT_ID(N'dbo.Game', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Game') AND name = N'IX_Game_GameYear')
        CREATE NONCLUSTERED INDEX IX_Game_GameYear ON dbo.Game (GameYear) WHERE GameYear IS NOT NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Game') AND name = N'IX_Game_Eco')
        CREATE NONCLUSTERED INDEX IX_Game_Eco ON dbo.Game (Eco) WHERE Eco IS NOT NULL;
END
GO
