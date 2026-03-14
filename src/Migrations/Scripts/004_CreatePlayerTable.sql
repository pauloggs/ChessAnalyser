-- Idempotent: create Player table if it does not exist.
IF OBJECT_ID(N'dbo.Player', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Player
    (
        Id        INT IDENTITY(1, 1) NOT NULL,
        Surname   NVARCHAR(200)       NOT NULL,
        Forenames NVARCHAR(400)       NOT NULL DEFAULT N'',
        CONSTRAINT PK_Player PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Player_Surname_Forenames UNIQUE NONCLUSTERED (Surname, Forenames)
    );
END
GO
