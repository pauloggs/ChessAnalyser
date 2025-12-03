namespace Interfaces.DTO
{
    public class PgnGame
    {
        /// <summary>
        /// The name of the parent PGN file associated with this PGN game content.
        /// </summary>
        public required string ParentPgnFileName { get; set; }

        /// <summary>
        /// Contents of the PGN game.
        /// </summary>
        public required string Contents { get; set; }
    }
}
