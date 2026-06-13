-- Schema: Ref.WorldChampion (18 canonical rows) + populations
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Ref')
    EXEC(N'CREATE SCHEMA Ref');
GO

IF OBJECT_ID(N'Ref.WorldChampion', N'U') IS NOT NULL
    DROP TABLE Ref.WorldChampion;
GO

CREATE TABLE Ref.WorldChampion (
    Id              INT            IDENTITY(1, 1) NOT NULL,
    Surname         NVARCHAR(200)  NOT NULL,
    Forenames       NVARCHAR(400)  NOT NULL,
    ChampionOrder   SMALLINT       NOT NULL,
    ReignStartYear  SMALLINT       NULL,
    ReignEndYear    SMALLINT       NULL,
    CONSTRAINT PK_Ref_WorldChampion PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Ref_WorldChampion_ChampionOrder UNIQUE (ChampionOrder),
    CONSTRAINT UQ_Ref_WorldChampion_Surname_Forenames UNIQUE (Surname, Forenames),
    CONSTRAINT CK_Ref_WorldChampion_Id_ChampionOrder CHECK (Id = ChampionOrder)
);
GO

IF NOT EXISTS (SELECT 1 FROM Ref.WorldChampion)
BEGIN
    SET IDENTITY_INSERT Ref.WorldChampion ON;

    INSERT INTO Ref.WorldChampion (Id, Surname, Forenames, ChampionOrder, ReignStartYear, ReignEndYear)
    VALUES
        ( 1, N'Steinitz',   N'Wilhelm',       1,  1886, 1894),
        ( 2, N'Lasker',     N'Emanuel',       2,  1894, 1921),
        ( 3, N'Capablanca', N'Jose Raul',     3,  1921, 1927),
        ( 4, N'Alekhine',   N'Alexander',     4,  1927, 1946),
        ( 5, N'Euwe',       N'Max',           5,  1935, 1937),
        ( 6, N'Botvinnik',  N'Mikhail',       6,  1948, 1963),
        ( 7, N'Smyslov',    N'Vasily',        7,  1957, 1958),
        ( 8, N'Tal',        N'Mikhail',       8,  1960, 1961),
        ( 9, N'Petrosian',  N'Tigran',        9,  1963, 1969),
        (10, N'Spassky',    N'Boris',        10,  1969, 1972),
        (11, N'Fischer',    N'Bobby',        11,  1972, 1975),
        (12, N'Karpov',     N'Anatoly',      12,  1975, 1985),
        (13, N'Kasparov',   N'Garry',        13,  1985, 2000),
        (14, N'Kramnik',    N'Vladimir',     14,  2000, 2007),
        (15, N'Anand',      N'Viswanathan',  15,  2007, 2013),
        (16, N'Carlsen',    N'Magnus',       16,  2013, 2023),
        (17, N'Ding',       N'Liren',        17,  2023, 2024),
        (18, N'Gukesh',     N'Dommaraju',    18,  2024, NULL);

    SET IDENTITY_INSERT Ref.WorldChampion OFF;
END
GO

-- Add WorldChampionId column and FK reference to the new Ref.WorldChampion table
IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'WorldChampionId'
          AND Object_ID = Object_ID(N'dbo.Player'))
BEGIN

	ALTER TABLE dbo.Player 
		ADD WorldChampionId INT NULL;

	ALTER TABLE dbo.Player
		ADD CONSTRAINT FK_Player_WorldChampion
		FOREIGN KEY (WorldChampionId) REFERENCES Ref.WorldChampion (Id);
END
GO

