-- Migration 015: FIDE catalog seed (DESIGN §13.4).
-- Data is inserted by the Migrations host from data/fide/players_list_foa.txt
-- (see SeedRefFidePlayerRunner.cs) when Ref.FidePlayer is empty.
-- A pure-SQL seed is not used: the official list is ~1.8M rows (~300 MB).

IF OBJECT_ID(N'Ref.FidePlayer', N'U') IS NULL
BEGIN
    RAISERROR('Ref.FidePlayer must exist before seed (run 014_CreateRefFidePlayer.sql).', 16, 1);
    RETURN;
END
GO
