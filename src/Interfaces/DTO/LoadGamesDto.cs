using System.ComponentModel;

namespace Interfaces.DTO
{
    public class LoadGamesDto
    {
        [DefaultValue("/Library/PGN")]
        public required string FilePath { get; set; } = "/Library/PGN";

        [DefaultValue(false)]
        public bool DisplayBoardPosition { get; set; } = false;
    }
}
