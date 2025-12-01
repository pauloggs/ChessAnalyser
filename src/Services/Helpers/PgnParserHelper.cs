using Interfaces;
using Interfaces.DTO;

namespace Services.Helpers
{
    public static class PgnParserHelper
    {
        /// <summary>
        /// Extracts a list of raw chess games from the contents of a PGN (Portable Game Notation) file.
        /// </summary>
        /// <remarks>This method identifies individual games in the PGN file by splitting the file's
        /// contents using a predefined game start marker. Each extracted game includes the original game start marker
        /// and is associated with the name of the PGN file it was extracted from.</remarks>
        /// <param name="rawPgn">The PGN file represented as a <see cref="RawPgn"/> object, containing the file name and its contents.</param>
        /// <returns>A list of <see cref="RawGame"/> objects, where each object represents a single raw chess game extracted from
        /// the PGN file.  If no games are found, an empty list is returned.</returns>
        public static List<RawGame> GetRawGamesFromPgnFile(RawPgn rawPgn)
        {
            var rawGames = new List<RawGame>();

            if (!rawPgn.Contents.Contains(Constants.GameStartMarker)) { return rawGames; }

            var pgnFileName = rawPgn.Name;

            string[] tokens = rawPgn.Contents.Split([Constants.GameStartMarker], StringSplitOptions.None);

            // Exit if there are no 'start of game' markers
            if (tokens.Length == 0) { return rawGames; }

            // Process each token
            foreach (string token in tokens)
            {
                // If the token has any contents, add to the raw games
                if (token.Length > 0)
                {
                    // Re-add the GameStartMarker
                    var gameContents = (Constants.GameStartMarker + token).Trim();

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
