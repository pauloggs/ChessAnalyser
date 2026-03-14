-- Idempotent: add WhitePlayerId and BlackPlayerId to Game as NOT NULL (tables assumed empty).
IF OBJECT_ID(N'dbo.Game', N'U') IS NULL OR OBJECT_ID(N'dbo.Player', N'U') IS NULL
    RETURN;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Game') AND name = N'WhitePlayerId')
    ALTER TABLE dbo.Game ADD WhitePlayerId INT NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Game') AND name = N'BlackPlayerId')
    ALTER TABLE dbo.Game ADD BlackPlayerId INT NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Game_WhitePlayer')
    ALTER TABLE dbo.Game ADD CONSTRAINT FK_Game_WhitePlayer FOREIGN KEY (WhitePlayerId) REFERENCES dbo.Player (Id);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Game_BlackPlayer')
    ALTER TABLE dbo.Game ADD CONSTRAINT FK_Game_BlackPlayer FOREIGN KEY (BlackPlayerId) REFERENCES dbo.Player (Id);
GO
