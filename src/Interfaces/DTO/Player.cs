namespace Interfaces.DTO
{
    /// <summary>
    /// Represents a chess player (dimension for analytics). Parsed from PGN White/Black tags.
    /// </summary>
    public class Player
    {
        public int Id { get; set; }

        /// <summary>Surname (family name). No leading or trailing spaces.</summary>
        public string Surname { get; set; } = string.Empty;

        /// <summary>Forenames (given names). No leading or trailing spaces. May be empty.</summary>
        public string Forenames { get; set; } = string.Empty;
    }
}
