
TRUNCATE TABLE [dbo].[BoardPosition];

DELETE [dbo].[Game];


SELECT count(*)
  FROM [Chess].[dbo].[Game]

select max(len(Gameid)) from dbo.Game;