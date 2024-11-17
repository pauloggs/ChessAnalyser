namespace Interfaces.DTO
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Game")]
    public class Game
    {
        public required string Name { get; set; }

        public Dictionary<string, string> Tags { get; set; }

        public Dictionary<int, Ply> Plies { get; set; }

        public Game()
        {
            Tags = [];
            Plies = [];
        }
    }
}
