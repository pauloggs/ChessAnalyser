/****** Object:  StoredProcedure [dbo].[DeleteGameById]    Script Date: 10/05/2026 16:24:35 ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
-- Explicit delete path for a game and dependent rows.
-- Avoids broad ON DELETE CASCADE behavior by requiring an intentional proc call.

CREATE   PROCEDURE dbo.DeleteGameById
    @GameId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @GameId IS NULL OR @GameId <= 0
    BEGIN
        THROW 50001, 'DeleteGameById: @GameId must be a positive integer.', 1;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Game WHERE Id = @GameId)
    BEGIN
        THROW 50002, 'DeleteGameById: game not found.', 1;
    END

    BEGIN TRANSACTION;

    DELETE FROM dbo.GamePositionSummary WHERE GameId = @GameId;
    DELETE FROM dbo.GameMove WHERE GameId = @GameId;
    DELETE FROM dbo.BoardPosition WHERE GameId = @GameId;
    DELETE FROM dbo.Game WHERE Id = @GameId;

    COMMIT TRANSACTION;
END

