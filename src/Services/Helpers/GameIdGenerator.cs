using Interfaces.DTO;
using System.Security.Cryptography;
using System.Text;

namespace Services.Helpers
{   
    public class GameIdGenerator
    {
        /// <summary>
        /// Generates a unique identifier for a sequence of moves in a game.
        /// </summary>
        /// <remarks>The generated identifier is based on the sequence of moves provided in the <paramref
        /// name="plies"/> dictionary.  The order of moves is determined by the dictionary's keys, and the hash is
        /// computed using the raw move data  of each <see cref="Ply"/> object. This ensures that the same sequence of
        /// moves always produces the same identifier.</remarks>
        /// <param name="plies">A dictionary where the key represents the move number and the value is a <see cref="Ply"/> object 
        /// containing the raw move data. The dictionary must not be null, and all <see cref="Ply.RawMove"/> values 
        /// must be non-null.</param>
        /// <returns>A string representing the SHA-256 hash of the concatenated raw move data, formatted as a hexadecimal string.</returns>
        public static string GetGameId(Dictionary<int, Ply> plies)
        {
            using SHA256 sha256Hash = SHA256.Create();

            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(string.Join("|", plies.Select(x => x.Value.RawMove))));
            
            StringBuilder builder = new StringBuilder();
            
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}

