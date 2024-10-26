using System.ComponentModel.DataAnnotations.Schema;

namespace Interfaces.DTO
{
    [Table("Game")]
    public class Game
    {
        public required string Name { get; set; }
    }
}
