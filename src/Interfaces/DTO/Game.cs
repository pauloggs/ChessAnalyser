using System.ComponentModel.DataAnnotations.Schema;

namespace Interfaces.DTO
{
    [Table("Game")]
    public class Game
    {
        public required string Name { get; set; }

        public required string Event { get; set; }

        public required string Site { get; set; }

        public required string Date { get; set; }

        public required string Round { get; set; }

        public required string White { get; set; }

        public required string Black { get; set; }

        public required string Result { get; set; }

        public required string WhiteElo { get; set; }

        public required string BlackElo { get; set; }

        public required string ECO { get; set; }
    }
}
