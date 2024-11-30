namespace Interfaces.DTO
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Game")]
    public class Game
    {
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
        /// Id is generated from a concatenation of all the moves in the game.
        /// </summary>
        public string? Id { get; set; }

        public Game()
        {
            Tags = new Dictionary<string, string>();
            Plies = new Dictionary<int, Ply>();
            BoardPositions = new Dictionary<int, BoardPosition>();
        }
    }
}
