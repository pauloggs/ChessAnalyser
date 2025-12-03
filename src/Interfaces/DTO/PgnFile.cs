using System.ComponentModel.DataAnnotations.Schema;

namespace Interfaces.DTO
{
    [Table("RawPgn")]
    public class PgnFile
    {
        /// <summary>
        /// Name of the PGN file
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Contents of the PGN file
        /// </summary>
        public required string Contents { get; set; }
    }
}
