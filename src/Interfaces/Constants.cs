using Interfaces.DTO;

namespace Interfaces
{
    public static class Constants
    {
        /// <summary>
        /// A list of standard PGN game tag identifiers, such as 'event', 'site', 'date', etc.
        /// </summary>
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

        /// <summary>
        /// An enumeration to represent pieces on a display board.
        /// </summary>
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

        /// <summary>
        /// An enumeration to represent the colour of players or pieces.
        /// </summary>
        public enum Colour
        {
            /// <summary>
            /// No colour / not applicable.
            /// </summary>
            N = 0,

            /// <summary>
            /// White colour.
            /// </summary>
            W = 1,

            /// <summary>
            /// Black colour.
            /// </summary>
            B = 2
        }

        /// <summary>
        /// Defines the marker that indicates the start of a new game in a PGN file.
        /// </summary>
        public static string GameStartMarker { get; } = "[Event";

        /// <summary>
        /// Defines a mapping from file characters ('a' to 'h') to their corresponding zero-based indices (0 to 7).
        /// </summary>
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

        /// <summary>
        /// Defines a mapping from zero-based file indices (0 to 7) to their corresponding file characters ('A' to 'H').
        /// </summary>
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

        /// <summary>
        /// Defines a mapping from piece characters to their corresponding indices.
        /// </summary>
        public static readonly Dictionary<char, int> PieceIndex = new()
        {
            { 'P', 0},
            { 'N', 1},
            { 'B', 2},
            { 'R', 3},
            { 'Q', 4},
            { 'K', 5}
         };
                
        private static readonly Dictionary<char, Piece> _pieces = new()
        {
            { 'X', new Piece('X', 0.0) }, // no move
            { 'P', new Piece('P', 1.0) },
            { 'N', new Piece('N', 3.0) },
            { 'B', new Piece('B', 3.0) },
            { 'R', new Piece('R', 5.0) },
            { 'Q', new Piece('Q', 9.0) },
            { 'K', new Piece('K', 1000.0) },
            { 'C', new Piece('C', 0.0) } // castling move
        };

        /// <summary>
        /// Defines a mapping from piece characters to their corresponding Piece objects, which include name and value.
        /// </summary>
        public static IReadOnlyDictionary<char, Piece> Pieces => _pieces;

        /// <summary>
        /// Defines the relative positions a knight can move to from its current position.
        /// </summary>
        public static readonly List<(int file, int rank)> RelativeKnightPositions =
        [
            (-2,-1),
            (-1,-2),
            (1,-2),
            (2,-1),

            (2,1),
            (1,2),
            (-1,2),
            (-2,1)
        ];
        
        private static readonly Dictionary<string, string> _gameEndConditions = new()
        {
            { "1-0", "W" },
            { "0-1", "B" },
            { "1/2-1/2", "D" }
        };

        /// <summary>
        /// Defines the mapping of game end conditions from PGN result strings to internal representations.
        /// </summary>
        public static IReadOnlyDictionary<string, string> GameEndConditions => _gameEndConditions;

        public static int MoveNotFound { get; } = -1;

        /// <summary>
        /// Standard algebraic notation square definitions as 0-63 indices.
        /// </summary>
        public static class Squares
        {
            // King-side castling squares
            // White side (Rank 1)
            public const int E1 = 4;
            public const int G1 = 6;
            public const int H1 = 7;
            public const int F1 = 5;

            // Black side (Rank 8)
            public const int E8 = 60;
            public const int G8 = 62;
            public const int H8 = 63;
            public const int F8 = 61;

            // Queen-side castling squares
            // White side (Rank 1)
            public const int A1 = 0; // New
            public const int C1 = 2; // New
            public const int D1 = 3; // New

            // Black side (Rank 8)
            public const int A8 = 56; // New
            public const int C8 = 58; // New
            public const int D8 = 59; // New
        }
    }
}
