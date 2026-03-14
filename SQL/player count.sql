USE Chess;

DECLARE @PlayerId INT = 1568;
DECLARE @PlayerSurname NVARCHAR(800) = 'Fischer';
DECLARE @PlayerForenames NVARCHAR(400) = 'Robert James';

SET @PlayerId 
	= COALESCE(@PlayerId, (SELECT TOP 1 Id FROM Chess.dbo.Player p WHERE p.Surname = @PlayerSurname AND p.Forenames = @PlayerForenames));
PRINT @PlayerId;
WITH WhiteOpponentPlayers AS (
	SELECT
		p.Id,
		COALESCE(COUNT(*), 0) AS PlayerCount
	FROM
		dbo.Game g
		INNER JOIN
		dbo.Player p ON p.Id = g.WhitePlayerId
	WHERE
		g.BlackPlayerId = @PlayerId
	GROUP BY
		p.Id
),
BlackOpponentPlayers AS (
	SELECT
		p.Id,
		COALESCE(COUNT(*), 0) AS PlayerCount
	FROM
		dbo.Game g
		INNER JOIN
		dbo.Player p ON p.Id = g.BlackPlayerId
	WHERE
		g.WhitePlayerId = @PlayerId
	GROUP BY
		p.Id
)
SELECT
	p.Surname,
	p.Forenames,
	COALESCE(w.PlayerCount, 0) AS WhitePlayerCount,
	COALESCE(b.PlayerCount, 0) AS BlackPlayerCount,
	COALESCE(w.PlayerCount, 0) + COALESCE(b.PlayerCount, 0) AS PlayerCount
FROM
	dbo.Player p
	LEFT JOIN
	WhiteOpponentPlayers w ON w.Id = p.Id
	LEFT JOIN
	BlackOpponentPlayers b ON b.Id = p.Id
ORDER BY
	COALESCE(w.PlayerCount, 0) + COALESCE(b.PlayerCount, 0) DESC,
	p.Surname,
	p.Forenames;