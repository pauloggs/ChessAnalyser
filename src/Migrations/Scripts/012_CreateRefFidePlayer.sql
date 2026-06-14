-- Idempotent: FIDE player catalog in Ref schema (DESIGN §13.4).

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Ref')
    EXEC(N'CREATE SCHEMA Ref');
GO

IF OBJECT_ID(N'Ref.FidePlayer', N'U') IS NULL
BEGIN
    CREATE TABLE Ref.FidePlayer (
        Id      INT            NOT NULL,
        Surname     NVARCHAR(200)  NOT NULL,
        Forenames   NVARCHAR(400)  NOT NULL,
        Federation  CHAR(3)        NULL,
        Sex         CHAR(1)        NULL,
        FideTitle   NVARCHAR(8)    NULL,
        BirthYear   SMALLINT       NULL,
        CONSTRAINT PK_Ref_FidePlayer PRIMARY KEY CLUSTERED (Id)
    );

    CREATE NONCLUSTERED INDEX IX_Ref_FidePlayer_Surname
        ON Ref.FidePlayer (Surname);
END
GO

-- Add FidePlayerId column and FK reference to the new Ref.FidePlayer table
IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'FidePlayerId'
          AND Object_ID = Object_ID(N'dbo.Player'))
BEGIN

	ALTER TABLE dbo.Player 
		ADD FidePlayerId INT NULL;

	ALTER TABLE dbo.Player
		ADD CONSTRAINT FK_Player_FidePlayer
		FOREIGN KEY (FidePlayerId) REFERENCES Ref.FidePlayer (Id);
END
GO