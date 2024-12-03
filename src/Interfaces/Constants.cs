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

        public enum DisplayBoardPiece
        {
            _ = 0,
            P = 1,
            N = 2,
            B = 3,
            R = 4,
            Q = 5,
            K = 6
        }

        static public string DefaultEmptyTagValue { get; } = "None";

        public static string GameStartMarker { get; } = "[Event";

        public static Dictionary<char, int> File { get; } = new Dictionary<char, int>()
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

        public static Dictionary<int, char> FileIds { get; } = new Dictionary<int, char>()
        {
            { 0, 'A' },
            { 1, 'B' },
            { 2, 'C' },
            { 3, 'D' },
            { 4, 'E' },
            { 5, 'F' },
            { 6, 'G' },
            { 7, 'H' }
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
            { 'X', new Piece(){ Name = 'X', Value = 0.0  } }, // no move
            { 'P', new Piece(){ Name = 'P', Value = 1.0  } },
            { 'N', new Piece(){ Name = 'N', Value = 3.0  } },
            { 'B', new Piece(){ Name = 'B', Value = 3.0  } },
            { 'R', new Piece(){ Name = 'R', Value = 5.0  } },
            { 'Q', new Piece(){ Name = 'Q', Value = 9.0  } },
            { 'K', new Piece(){ Name = 'K', Value = 1000.0  } },
            { 'C', new Piece(){ Name = 'C', Value = 0.0  } } // castling move
        };
    }
}
