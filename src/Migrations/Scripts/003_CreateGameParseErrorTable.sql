-- Idempotent: create GameParseError table for persisting parse failures.
-- Does not reference Game (failed games are not inserted). Enables diagnostics without interrupting persistence.
IF OBJECT_ID(N'dbo.GameParseError', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GameParseError
    (
        Id                  INT IDENTITY(1, 1) NOT NULL,
        SourcePgnFileName   NVARCHAR(500)       NULL,
        GameIndexInFile     INT                 NULL,
        GameName            NVARCHAR(500)       NULL,
        ErrorMessage        NVARCHAR(MAX)       NOT NULL,
        RecordedAt          DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_GameParseError PRIMARY KEY CLUSTERED (Id)
    );
END
GO
