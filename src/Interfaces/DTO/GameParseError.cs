namespace Interfaces.DTO
{
    /// <summary>
    /// Represents a game that failed to parse (illegal move, ambiguous move, or invalid PGN).
    /// Persisted for diagnostics without interrupting the persistence of valid games.
    /// </summary>
    public class GameParseError
    {
        /// <summary>Source PGN file name.</summary>
        public string? SourcePgnFileName { get; set; }

        /// <summary>1-based index of the game within the source file.</summary>
        public int? GameIndexInFile { get; set; }

        /// <summary>Game name (e.g. from Event tag).</summary>
        public string? GameName { get; set; }

        /// <summary>Error message from the parser.</summary>
        public required string ErrorMessage { get; set; }
    }
}
