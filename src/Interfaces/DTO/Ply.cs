namespace Interfaces.DTO
{
    public class Ply
    {
        public int MoveNumber { get; set; }
        public required string RawMove { get; set; }
        public char Colour { get; set; }
        public bool IsPieceMove { get; set; }
        public bool IsPawnMove { get; set; }
        public bool IsKingsideCastling { get; set; }
        public bool IsQueensideCastling { get; set; }
        public bool IsEnpassant { get; set; }
        public bool IsCheck { get; set; }
        public bool IsCapture { get; set; }
    }
}