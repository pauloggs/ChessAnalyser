use Chess;


TRUNCATE TABLE [App].[BoardPosition];

DELETE [App].[Game];

TRUNCATE TABLE [App].[GameParseError];
delete [App].[Player];

SELECT count(*)
  FROM App.Game

select max(len(Gameid)) from App.Game;
