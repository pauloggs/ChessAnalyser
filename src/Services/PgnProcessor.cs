using Interfaces.DTO;

namespace Services
{
    public interface IPgnProcessor
    {
        /// <summary>
        /// Splits each of the provided PGN file contents into separate games.
        /// </summary>
        /// <param name="rawPgns"></param>
        /// <returns></returns>
        List<RawGame> GetRawGamesFromPgnFiles(List<RawPgn> rawPgns);

        List<Game> GetGamesFromRawGames(List<RawGame> rawGames);
    }

    public class PgnProcessor(IParser parser, INaming naming) : IPgnProcessor
    {
        private readonly IParser parser = parser;
        private readonly INaming naming = naming;

        public List<RawGame> GetRawGamesFromPgnFiles(List<RawPgn> rawPgns)
        {
            var rawGames = new List<RawGame>();

            foreach (var rawPgn in rawPgns)
            {
                var rawGamesForPgnFile = parser.GetRawGamesFromPgnFile(rawPgn);

                rawGames.AddRange(rawGamesForPgnFile);
            }

            return rawGames;
        }

        public List<Game> GetGamesFromRawGames(List<RawGame> rawGames)
        {
            var games = new List<Game>();
            foreach (var rawGame in rawGames)
            {
                // get the tags and the raw moves
                var gameTags = parser.GetGameTags(rawGame.Contents);

                var gameName = naming.GetGameName(gameTags);

                var game = new Game()
                {
                    Name = gameName,
                    Event = gameTags["Event"],
                    Site = gameTags["Site"],
                    Date = gameTags["Date"],
                    Round = gameTags["Round"],
                    White = gameTags["White"],
                    Black = gameTags["Black"],
                    Result = gameTags["Result"],
                    WhiteElo = gameTags["WhiteElo"],
                    BlackElo = gameTags["BlackElo"],
                    ECO = gameTags["ECO"]
                };

                // get the tag section


                games.Add(game);
            }
            return games;
        }
    }
}
