-- Idempotent: FIDE player catalog in Ref schema (DESIGN §13.4).
-- Populated by --import-fide-catalog (official rating list TXT); not seeded here.

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Ref')
    EXEC(N'CREATE SCHEMA Ref');
GO

IF OBJECT_ID(N'Ref.FidePlayer', N'U') IS NULL
BEGIN
    CREATE TABLE Ref.FidePlayer (
        FideId      INT            NOT NULL,
        Surname     NVARCHAR(200)  NOT NULL,
        Forenames   NVARCHAR(400)  NOT NULL,
        Federation  CHAR(3)        NULL,
        Sex         CHAR(1)        NULL,
        FideTitle   NVARCHAR(8)    NULL,
        BirthYear   SMALLINT       NULL,
        CONSTRAINT PK_Ref_FidePlayer PRIMARY KEY CLUSTERED (FideId)
    );

    CREATE NONCLUSTERED INDEX IX_Ref_FidePlayer_Surname
        ON Ref.FidePlayer (Surname);
END
GO
