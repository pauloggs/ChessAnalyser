-- Idempotent: reference schema and classical world-champion name rows (DESIGN §13).
-- Source: FIDE classical world championship line (see chess.com / FIDE WCC records).

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Ref')
    EXEC(N'CREATE SCHEMA Ref');
GO

IF OBJECT_ID(N'Ref.WorldChampion', N'U') IS NULL
BEGIN
    CREATE TABLE Ref.WorldChampion (
        Id              INT            IDENTITY(1, 1) NOT NULL,
        Surname         NVARCHAR(200)  NOT NULL,
        Forenames       NVARCHAR(400)  NOT NULL,
        ChampionOrder   SMALLINT       NOT NULL,
        ReignStartYear  SMALLINT       NULL,
        ReignEndYear    SMALLINT       NULL,
        CONSTRAINT PK_Ref_WorldChampion PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Ref_WorldChampion_Surname_Forenames UNIQUE (Surname, Forenames)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM Ref.WorldChampion)
BEGIN
    INSERT INTO Ref.WorldChampion (Surname, Forenames, ChampionOrder, ReignStartYear, ReignEndYear)
    VALUES
        (N'Steinitz',   N'Wilhelm',         1,  1886, 1894),
        (N'Lasker',     N'Emanuel',         2,  1894, 1921),
        (N'Capablanca', N'Jose Raul',       3,  1921, 1927),
        (N'Alekhine',   N'Alexander',       4,  1927, 1946),
        (N'Euwe',       N'Max',             5,  1935, 1937),
        (N'Botvinnik',  N'Mikhail',         6,  1948, 1963),
        (N'Smyslov',    N'Vasily',          7,  1957, 1958),
        (N'Tal',        N'Mikhail',         8,  1960, 1961),
        (N'Petrosian',  N'Tigran',          9,  1963, 1969),
        (N'Spassky',    N'Boris',          10,  1969, 1972),
        (N'Fischer',    N'Bobby',          11,  1972, 1975),
        (N'Fischer',    N'Robert James',   11,  1972, 1975),
        (N'Karpov',     N'Anatoly',        12,  1975, 1985),
        (N'Kasparov',   N'Garry',          13,  1985, 2000),
        (N'Kramnik',    N'Vladimir',       14,  2000, 2007),
        (N'Anand',      N'Viswanathan',    15,  2007, 2013),
        (N'Carlsen',    N'Magnus',         16,  2013, 2023),
        (N'Ding',       N'Liren',          17,  2023, 2024),
        (N'Gukesh',     N'Dommaraju',      18,  2024, NULL);
END
GO
