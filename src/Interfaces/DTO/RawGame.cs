namespace Interfaces.DTO
{
    public class RawGame
    {
        public string ParentPgnFileName { get; set; }

        public string GameName { get; set; }

        public required string Contents { get; set; }
    }
}
