namespace Interfaces.DTO
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Holds information about a complete chess game.
    /// </summary>
    [Table("Game")]
    public class Game
    {
        public int Id { get; set; }

        /// <summary>
        /// The name of the game, typically derived from the Event tag in PGN.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Represents a dictionary of all the retrieved tags from the
        /// PGN file, such as Event, Round etc.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }

        /// <summary>
        /// Represents a dictionary of each Ply at each
        /// Ply number of the game.
        /// </summary>
        public Dictionary<int, Ply> Plies { get; set; }

        /// <summary>
        /// Represents a dictionary of BoardPosition objects at
        /// each Ply number of the game.
        /// </summary>
        public Dictionary<int, BoardPosition> BoardPositions { get; set; }

        /// <summary>
        /// GameMoves is generated from a concatenation of all the moves in the game.
        /// </summary>
        public string GameId { get; set; }

        public string Winner { get; set; }

        public Game()
        {
            Tags = [];
            Plies = [];
            BoardPositions = [];
            Winner = "None";
            GameId = string.Empty;
        }
    }
}
