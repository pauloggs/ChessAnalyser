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
        /// Represents a dictionary of each Ply at each Ply number of the game.
        /// For example, Ply 0 and Ply 1 represent the first move by White and Black respectively.
        /// </summary>
        public Dictionary<int, Ply> Plies { get; set; }

        /// <summary>
        /// Represents a dictionary of BoardPosition objects at each zero-index Ply of the game.
        /// Ply 0 represents  first move by White, which is the first position after the starting position.
        /// </summary>
        public Dictionary<int, BoardPosition> BoardPositions { get; set; }

        /// <summary>
        /// The starting board position before any moves are made.
        /// This represents the initial arrangement of pieces at the beginning of the game.
        /// </summary>
        public BoardPosition? InitialBoardPosition { get; set; }

        /// <summary>
        /// GameMoves is generated from a concatenation of all the moves in the game.
        /// </summary>
        public string GameId { get; set; }

        public string Winner { get; set; }

        /// <summary>
        /// Source PGN file name (when loaded from file). Used for error reporting during parsing.
        /// </summary>
        public string? SourcePgnFileName { get; set; }

        /// <summary>
        /// 1-based index of this game within the source PGN file. Used for error reporting during parsing.
        /// </summary>
        public int? GameIndexInFile { get; set; }

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
