using Interfaces.DTO;

namespace Interfaces
{
    public static class Constants
    {
        public static List<string> GameTagIdentifiers { get; } =
        [
            "event",
            "site",
            "date",
            "round",
            "white",
            "black",
            "result",
            "whiteelo",
            "blackelo",
            "eco"
        ];

        static public string DefaultEmptyTagValue { get; } = "None";

        public static string GameStartMarker { get; } = "[Event";

        public static Dictionary<char, int> FileLookup { get; } = new Dictionary<char, int>()
        {
            { 'a', 0 },
            { 'b', 1 },
            { 'c', 2 },
            { 'd', 3 },
            { 'e', 4 },
            { 'f', 5 },
            { 'g', 6 },
            { 'h', 7 }
        };

        public static Dictionary<char, int> PieceIndex = new()
        {
            { 'P', 0},
            { 'N', 1},
            { 'B', 2},
            { 'R', 3},
            { 'Q', 4},
            { 'K', 5}
         };        

        public static Dictionary<char, Piece> Pieces = new()
        {
            { 'P', new Piece(){ Name = "Pawn", Value = 1.0  } },
            { 'N', new Piece(){ Name = "Knight", Value = 3.0  } },
            { 'B', new Piece(){ Name = "Bishop", Value = 3.0  } },
            { 'R', new Piece(){ Name = "Rook", Value = 5.0  } },
            { 'Q', new Piece(){ Name = "Queen", Value = 9.0  } },
            { 'K', new Piece(){ Name = "King", Value = 1000.0  } },

        };
    }
}
