namespace Interfaces.DTO
{
    public class Ply
    {
        public int MoveNumber { get; set; }
        public required string RawMove { get; set; }
        public char Colour { get; set; }
    }
}