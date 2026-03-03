-- Idempotent: create Game table if it does not exist.
IF OBJECT_ID(N'dbo.Game', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Game
    (
        Id     INT IDENTITY(1, 1) NOT NULL,
        Name   NVARCHAR(500)       NOT NULL,
        GameId NVARCHAR(100)       NOT NULL,
        Winner NVARCHAR(10)        NOT NULL DEFAULT N'None',
        CONSTRAINT PK_Game PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Game_GameId UNIQUE NONCLUSTERED (GameId)
    );
END
GO
