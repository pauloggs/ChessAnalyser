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

        /// <summary>True when this player has held the classical world championship (curated catalog).</summary>
        public bool WasWorldChampion { get; set; }

        /// <summary>FIDE player ID when matched from an official rating list; null when unknown.</summary>
        public int? FideId { get; set; }

        /// <summary>Three-letter federation code from FIDE (e.g. NOR, IND).</summary>
        public string? Federation { get; set; }

        /// <summary>M or F when known from FIDE.</summary>
        public string? Sex { get; set; }

        /// <summary>Highest FIDE title when known: GM, IM, WGM, FM, WFM, CM, WCM.</summary>
        public string? FideTitle { get; set; }

        /// <summary>Birth year from FIDE when known.</summary>
        public short? BirthYear { get; set; }
    }
}
