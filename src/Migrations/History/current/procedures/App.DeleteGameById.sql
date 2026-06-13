SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
-- Explicit delete path for a game and dependent rows.
-- Avoids broad ON DELETE CASCADE behavior by requiring an intentional proc call.

CREATE   PROCEDURE App.DeleteGameById
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
