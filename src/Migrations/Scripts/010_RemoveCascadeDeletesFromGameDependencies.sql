-- Strict no-cascade policy: ensure child tables referencing dbo.Game do not use ON DELETE CASCADE.
-- This migration is idempotent and safe to run repeatedly.

IF OBJECT_ID(N'dbo.Game', N'U') IS NULL
    RETURN;
GO

-- dbo.BoardPosition -> dbo.Game
IF OBJECT_ID(N'dbo.BoardPosition', N'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID(N'dbo.BoardPosition')
          AND referenced_object_id = OBJECT_ID(N'dbo.Game')
          AND name = N'FK_BoardPosition_Game'
          AND delete_referential_action = 1
    )
    BEGIN
        ALTER TABLE dbo.BoardPosition DROP CONSTRAINT FK_BoardPosition_Game;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID(N'dbo.BoardPosition')
          AND referenced_object_id = OBJECT_ID(N'dbo.Game')
          AND name = N'FK_BoardPosition_Game'
    )
    BEGIN
        ALTER TABLE dbo.BoardPosition
            ADD CONSTRAINT FK_BoardPosition_Game FOREIGN KEY (GameId) REFERENCES dbo.Game (Id);
    END
END
GO

-- dbo.GameMove -> dbo.Game
IF OBJECT_ID(N'dbo.GameMove', N'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID(N'dbo.GameMove')
          AND referenced_object_id = OBJECT_ID(N'dbo.Game')
          AND name = N'FK_GameMove_Game'
          AND delete_referential_action = 1
    )
    BEGIN
        ALTER TABLE dbo.GameMove DROP CONSTRAINT FK_GameMove_Game;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID(N'dbo.GameMove')
          AND referenced_object_id = OBJECT_ID(N'dbo.Game')
          AND name = N'FK_GameMove_Game'
    )
    BEGIN
        ALTER TABLE dbo.GameMove
            ADD CONSTRAINT FK_GameMove_Game FOREIGN KEY (GameId) REFERENCES dbo.Game (Id);
    END
END
GO

-- dbo.GamePositionSummary -> dbo.Game
IF OBJECT_ID(N'dbo.GamePositionSummary', N'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID(N'dbo.GamePositionSummary')
          AND referenced_object_id = OBJECT_ID(N'dbo.Game')
          AND name = N'FK_GamePositionSummary_Game'
          AND delete_referential_action = 1
    )
    BEGIN
        ALTER TABLE dbo.GamePositionSummary DROP CONSTRAINT FK_GamePositionSummary_Game;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID(N'dbo.GamePositionSummary')
          AND referenced_object_id = OBJECT_ID(N'dbo.Game')
          AND name = N'FK_GamePositionSummary_Game'
    )
    BEGIN
        ALTER TABLE dbo.GamePositionSummary
            ADD CONSTRAINT FK_GamePositionSummary_Game FOREIGN KEY (GameId) REFERENCES dbo.Game (Id);
    END
END
GO
