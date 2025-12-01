using Interfaces;
using Interfaces.DTO;
using Microsoft.IdentityModel.Tokens;

namespace Services.Helpers
{
    public static class PgnParserHelper
    {
        public static List<RawGame> GetRawGamesFromPgnFile(RawPgn rawPgn)
        {
            var rawGames = new List<RawGame>();

            if (!rawPgn.Contents.Contains(Constants.GameStartMarker)) { return rawGames; }

            var pgnFileName = rawPgn.Name;

            string[] tokens = rawPgn.Contents.Split([Constants.GameStartMarker], StringSplitOptions.None);

            foreach (string token in tokens)
            {
                if (token.Length > 0)
                {
                    var gameContents = (Constants.GameStartMarker + token).Trim();

                    if (gameContents.IsNullOrEmpty()) { continue; }

                    rawGames.Add(new RawGame()
                    {
                        ParentPgnFileName = pgnFileName,
                        Contents = gameContents
                    });
                }
            }

            return rawGames;
        }

        
    }
}
