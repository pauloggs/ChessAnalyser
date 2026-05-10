-- Rollup fact table: one row per game position (ply) with precomputed scalar metrics.
-- Idempotent migration: safe to run multiple times.

IF OBJECT_ID(N'dbo.GamePositionSummary', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GamePositionSummary
    (
        GameId             INT       NOT NULL,
        PlyIndex           INT       NOT NULL,
        WhiteMaterial      SMALLINT  NOT NULL,
        BlackMaterial      SMALLINT  NOT NULL,
        WhitePawnCount     SMALLINT  NOT NULL,
        WhiteKnightCount   SMALLINT  NOT NULL,
        WhiteBishopCount   SMALLINT  NOT NULL,
        WhiteRookCount     SMALLINT  NOT NULL,
        WhiteQueenCount    SMALLINT  NOT NULL,
        WhiteKingCount     SMALLINT  NOT NULL,
        BlackPawnCount     SMALLINT  NOT NULL,
        BlackKnightCount   SMALLINT  NOT NULL,
        BlackBishopCount   SMALLINT  NOT NULL,
        BlackRookCount     SMALLINT  NOT NULL,
        BlackQueenCount    SMALLINT  NOT NULL,
        BlackKingCount     SMALLINT  NOT NULL,
        CONSTRAINT PK_GamePositionSummary PRIMARY KEY CLUSTERED (GameId, PlyIndex),
        CONSTRAINT FK_GamePositionSummary_Game FOREIGN KEY (GameId) REFERENCES dbo.Game (Id)
    );
END
GO

IF OBJECT_ID(N'dbo.GamePositionSummary', N'U') IS NOT NULL
    AND NOT EXISTS (SELECT 1
                    FROM sys.indexes
                    WHERE object_id = OBJECT_ID(N'dbo.GamePositionSummary')
                      AND name = N'IX_GamePositionSummary_PlyIndex')
BEGIN
    CREATE NONCLUSTERED INDEX IX_GamePositionSummary_PlyIndex
        ON dbo.GamePositionSummary (PlyIndex)
        INCLUDE (WhiteMaterial, BlackMaterial, GameId);
END
GO
