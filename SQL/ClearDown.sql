use Chess;


TRUNCATE TABLE [dbo].[BoardPosition];

DELETE [dbo].[Game];

TRUNCATE TABLE [dbo].[GameParseError];
delete [dbo].[Player];

SELECT count(*)
  FROM [Chess].[dbo].[Game]

select max(len(Gameid)) from dbo.Game;