-- Idempotent: FIDE-sourced metadata columns on dbo.Player (DESIGN §13, PLAN §15.1).

IF OBJECT_ID(N'dbo.Player', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Player', N'FideId') IS NULL
        ALTER TABLE dbo.Player ADD FideId INT NULL;

    IF COL_LENGTH(N'dbo.Player', N'Federation') IS NULL
        ALTER TABLE dbo.Player ADD Federation CHAR(3) NULL;

    IF COL_LENGTH(N'dbo.Player', N'Sex') IS NULL
        ALTER TABLE dbo.Player ADD Sex CHAR(1) NULL;

    IF COL_LENGTH(N'dbo.Player', N'FideTitle') IS NULL
        ALTER TABLE dbo.Player ADD FideTitle NVARCHAR(8) NULL;

    IF COL_LENGTH(N'dbo.Player', N'BirthYear') IS NULL
        ALTER TABLE dbo.Player ADD BirthYear SMALLINT NULL;
END
GO

IF OBJECT_ID(N'dbo.Player', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Player') AND name = N'UX_Player_FideId')
        CREATE UNIQUE NONCLUSTERED INDEX UX_Player_FideId ON dbo.Player (FideId) WHERE FideId IS NOT NULL;
END
GO
