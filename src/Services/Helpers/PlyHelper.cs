using Interfaces.DTO;
using System.Text.RegularExpressions;
using static Interfaces.Constants;

namespace Services.Helpers
{
    public static class PlyHelper
    {
        /// <summary>
        /// Parses a line of chess moves and adds the resulting plies to the specified dictionary.
        /// </summary>
        /// <remarks>Each move in the input string is parsed into a <see cref="Ply"/> object, which
        /// includes the move number, the raw move string, and the color of the player making the move ('W' for White,
        /// 'B' for Black). The method assumes that the ply number starts at the correct value for the first move in the
        /// input string.</remarks>
        /// <param name="plyDictionary">A dictionary where the key is the ply number and the value is the corresponding <see cref="Ply"/> object.
        /// The method adds new entries to this dictionary.</param>
        /// <param name="line">A string containing chess moves, separated by spaces. Move numbers (e.g., "1.", "2.") are ignored during
        /// parsing.</param>
        /// <param name="plyNumber">A reference to the current ply number. This value is incremented for each parsed ply and updated to reflect
        /// the next ply number.</param>
        public static void AddPlies(Dictionary<int, Ply> plyDictionary, string line, ref int plyNumber)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            // 1. Remove comments in braces: { ... }
            line = Regex.Replace(line, @"\{[^}]*\}", " ");

            // 2. Remove ';' comments to end of line
            line = Regex.Replace(line, @";.*$", " ");

            // 3. Remove move numbers (e.g., "1.", "2.", "1...") from the line
            line = Regex.Replace(line, @"\d+\.(\.\.)?", " ");

            // 4. Split the line into individual tokens based on whitespace
            var plies = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (string rawToken in plies)
            {
                var plyString = rawToken.Trim();

                // Skip empty tokens
                if (string.IsNullOrWhiteSpace(plyString))
                {
                    continue;
                }

                // Skip NAGs like $1, $5, etc.
                if (IsNagToken(plyString))
                {
                    continue;
                }

                // Skip ellipsis tokens ("...")
                if (plyString == "...")
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(plyString))
                {
                    var ply = new Ply()
                    {
                        MoveNumber = (plyNumber / 2) + 1, // Integer division to get the move number.
                        RawMove = plyString, // Store the raw move string. This will be parsed later.
                        Colour = plyNumber % 2 == 0 ? Colour.W : Colour.B // Even ply numbers are White's moves.
                    };

                    plyDictionary[plyNumber] = ply; // Add the ply to the dictionary.

                    plyNumber++; // Increment the ply number for the next move.
                }
            }
        }

        private static bool IsNagToken(string token)
        {
            if (string.IsNullOrEmpty(token) || token[0] != '$' || token.Length == 1)
            {
                return false;
            }

            for (int i = 1; i < token.Length; i++)
            {
                if (!char.IsDigit(token[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
