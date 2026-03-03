-- Ensure dbo.Game has a primary key (required for FK from BoardPosition).
-- Handles the case where Game was created manually without a PK.
IF OBJECT_ID(N'dbo.Game', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.Game') AND type = 'PK')
BEGIN
    ALTER TABLE dbo.Game ADD CONSTRAINT PK_Game PRIMARY KEY CLUSTERED (Id);
END
GO

-- Idempotent: create BoardPosition table for bitboard storage if it does not exist.
-- Stores one row per (Game, PlyIndex). PlyIndex -1 = initial position; 0,1,2,... = after each ply.
-- Each bitboard is a 64-bit unsigned integer (BIGINT in SQL Server).
IF OBJECT_ID(N'dbo.BoardPosition', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BoardPosition
    (
        GameId               INT           NOT NULL,
        PlyIndex             INT           NOT NULL,
        -- White piece bitboards (square index 0 = a1, 63 = h8)
        WP                   BIGINT        NOT NULL DEFAULT 0,
        WN                   BIGINT        NOT NULL DEFAULT 0,
        WB                   BIGINT        NOT NULL DEFAULT 0,
        WR                   BIGINT        NOT NULL DEFAULT 0,
        WQ                   BIGINT        NOT NULL DEFAULT 0,
        WK                   BIGINT        NOT NULL DEFAULT 0,
        -- Black piece bitboards
        BP                   BIGINT        NOT NULL DEFAULT 0,
        BN                   BIGINT        NOT NULL DEFAULT 0,
        BB                   BIGINT        NOT NULL DEFAULT 0,
        BR                   BIGINT        NOT NULL DEFAULT 0,
        BQ                   BIGINT        NOT NULL DEFAULT 0,
        BK                   BIGINT        NOT NULL DEFAULT 0,
        -- En-passant target file (a..h or NULL)
        EnPassantTargetFile  CHAR(1)       NULL,
        CONSTRAINT PK_BoardPosition PRIMARY KEY CLUSTERED (GameId, PlyIndex),
        CONSTRAINT FK_BoardPosition_Game FOREIGN KEY (GameId) REFERENCES dbo.Game (Id) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX IX_BoardPosition_GameId
        ON dbo.BoardPosition (GameId)
        INCLUDE (PlyIndex);
END
GO
