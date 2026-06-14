-- Idempotent: move operational tables from dbo to App schema.
-- Ref.* and dbo.SchemaVersions (DbUp journal) are unchanged.

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'App')
    EXEC(N'CREATE SCHEMA App');
GO

IF OBJECT_ID(N'dbo.Player', N'U') IS NOT NULL
    ALTER SCHEMA App TRANSFER dbo.Player;
GO

IF OBJECT_ID(N'dbo.Game', N'U') IS NOT NULL
    ALTER SCHEMA App TRANSFER dbo.Game;
GO

IF OBJECT_ID(N'dbo.BoardPosition', N'U') IS NOT NULL
    ALTER SCHEMA App TRANSFER dbo.BoardPosition;
GO

IF OBJECT_ID(N'dbo.GameMove', N'U') IS NOT NULL
    ALTER SCHEMA App TRANSFER dbo.GameMove;
GO

IF OBJECT_ID(N'dbo.GamePositionSummary', N'U') IS NOT NULL
    ALTER SCHEMA App TRANSFER dbo.GamePositionSummary;
GO

IF OBJECT_ID(N'dbo.GameParseError', N'U') IS NOT NULL
    ALTER SCHEMA App TRANSFER dbo.GameParseError;
GO

IF OBJECT_ID(N'dbo.DeleteGameById', N'P') IS NOT NULL
    DROP PROCEDURE dbo.DeleteGameById;
GO

CREATE OR ALTER PROCEDURE App.DeleteGameById
    @GameId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @GameId IS NULL OR @GameId <= 0
    BEGIN
        THROW 50001, 'DeleteGameById: @GameId must be a positive integer.', 1;
    END

    IF NOT EXISTS (SELECT 1 FROM App.Game WHERE Id = @GameId)
    BEGIN
        THROW 50002, 'DeleteGameById: game not found.', 1;
    END

    BEGIN TRANSACTION;

    DELETE FROM App.GamePositionSummary WHERE GameId = @GameId;
    DELETE FROM App.GameMove WHERE GameId = @GameId;
    DELETE FROM App.BoardPosition WHERE GameId = @GameId;
    DELETE FROM App.Game WHERE Id = @GameId;

    COMMIT TRANSACTION;
END
GO
