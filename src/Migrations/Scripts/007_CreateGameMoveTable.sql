-- Secondary fact table: one row per half-move (ply) for analytics queries.
-- Idempotent migration: safe to run multiple times.

IF OBJECT_ID(N'dbo.GameMove', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GameMove
    (
        GameId               INT        NOT NULL,
        PlyIndex             INT        NOT NULL,
        MovingSide           CHAR(1)    NOT NULL,
        FromSquare           TINYINT    NOT NULL,
        ToSquare             TINYINT    NOT NULL,
        MovedPiece           CHAR(1)    NOT NULL,
        CapturedPiece        CHAR(1)    NULL,
        PromotionPiece       CHAR(1)    NULL,
        IsCastlingKingside   BIT        NOT NULL CONSTRAINT DF_GameMove_IsCastlingKingside DEFAULT (0),
        IsCastlingQueenside  BIT        NOT NULL CONSTRAINT DF_GameMove_IsCastlingQueenside DEFAULT (0),
        CONSTRAINT PK_GameMove PRIMARY KEY CLUSTERED (GameId, PlyIndex),
        CONSTRAINT FK_GameMove_Game FOREIGN KEY (GameId) REFERENCES dbo.Game (Id),
        CONSTRAINT CK_GameMove_MovingSide CHECK (MovingSide IN ('W', 'B')),
        CONSTRAINT CK_GameMove_FromSquare CHECK (FromSquare BETWEEN 0 AND 63),
        CONSTRAINT CK_GameMove_ToSquare CHECK (ToSquare BETWEEN 0 AND 63),
        CONSTRAINT CK_GameMove_MovedPiece CHECK (MovedPiece IN ('P', 'N', 'B', 'R', 'Q', 'K')),
        CONSTRAINT CK_GameMove_CapturedPiece CHECK (CapturedPiece IS NULL OR CapturedPiece IN ('P', 'N', 'B', 'R', 'Q', 'K')),
        CONSTRAINT CK_GameMove_PromotionPiece CHECK (PromotionPiece IS NULL OR PromotionPiece IN ('N', 'B', 'R', 'Q'))
    );
END
GO

IF OBJECT_ID(N'dbo.GameMove', N'U') IS NOT NULL
    AND NOT EXISTS (SELECT 1
                    FROM sys.indexes
                    WHERE object_id = OBJECT_ID(N'dbo.GameMove')
                      AND name = N'IX_GameMove_ToSquare_MovedPiece')
BEGIN
    CREATE NONCLUSTERED INDEX IX_GameMove_ToSquare_MovedPiece
        ON dbo.GameMove (ToSquare, MovedPiece)
        INCLUDE (GameId, PlyIndex, MovingSide);
END
GO
