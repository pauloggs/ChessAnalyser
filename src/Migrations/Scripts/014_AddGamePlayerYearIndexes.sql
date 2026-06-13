-- Speed up min game-year lookups for player metadata linking (DESIGN §13.5).
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'App.Game') AND name = N'IX_Game_WhitePlayerId_GameYear')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Game_WhitePlayerId_GameYear
        ON App.Game (WhitePlayerId, GameYear)
        WHERE GameYear IS NOT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'App.Game') AND name = N'IX_Game_BlackPlayerId_GameYear')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Game_BlackPlayerId_GameYear
        ON App.Game (BlackPlayerId, GameYear)
        WHERE GameYear IS NOT NULL;
END
GO
