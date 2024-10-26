using System.ComponentModel.DataAnnotations.Schema;

namespace Interfaces.DTO
{
    [Table("RawPgn")]
    public class RawPgn
    {
        public required string Name { get; set; }
        public required string Contents { get; set; }
    }
}
