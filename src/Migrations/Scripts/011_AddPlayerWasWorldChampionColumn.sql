-- Idempotent: add WasWorldChampion flag to dbo.Player.
IF COL_LENGTH(N'dbo.Player', N'WasWorldChampion') IS NULL
BEGIN
    ALTER TABLE dbo.Player
        ADD WasWorldChampion BIT NOT NULL
            CONSTRAINT DF_Player_WasWorldChampion DEFAULT (0);
END
GO
