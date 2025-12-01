using Interfaces.DTO;
using System.Text.RegularExpressions;

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
            Regex moveNumbersRegex = new Regex(@"\d+\.");

            line = moveNumbersRegex.Replace(line, "");

            var plies = line.Split(" ");

            foreach (string plyString in plies)
            {
                if (!string.IsNullOrWhiteSpace(plyString))
                {
                    var ply = new Ply()
                    {
                        MoveNumber = (plyNumber / 2) + 1,
                        RawMove = plyString,
                        Colour = plyNumber % 2 == 0 ? 'W' : 'B'
                    };

                    plyDictionary[plyNumber] = ply;

                    plyNumber++;
                }
            }
        }
    }
}
