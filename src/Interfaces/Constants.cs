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

        /// <summary>
        /// Defines a mapping from piece characters to their corresponding Piece objects, which include name and value.
        /// </summary>
        public static readonly Dictionary<char, Piece> Pieces = new()
        {
            { 'X', new Piece(name: 'X', value: 0.0)}, // no move
            { 'P', new Piece(name: 'P', value: 1.0) },
            { 'N', new Piece(name: 'N', value: 3.0) },
            { 'B', new Piece(name: 'B', value: 3.0) },
            { 'R', new Piece(name: 'R', value: 5.0) },
            { 'Q', new Piece(name: 'Q', value: 9.0) },
            { 'K', new Piece(name: 'K', value: 1000.0) },
            { 'C', new Piece(name: 'C', value: 0.0) } // castling move

            //{ 'X', new Piece(){ Name = 'X', Value = 0.0  } }, // no move
            //{ 'P', new Piece(){ Name = 'P', Value = 1.0  } },
            //{ 'N', new Piece(){ Name = 'N', Value = 3.0  } },
            //{ 'B', new Piece(){ Name = 'B', Value = 3.0  } },
            //{ 'R', new Piece(){ Name = 'R', Value = 5.0  } },
            //{ 'Q', new Piece(){ Name = 'Q', Value = 9.0  } },
            //{ 'K', new Piece(){ Name = 'K', Value = 1000.0  } },
            //{ 'C', new Piece(){ Name = 'C', Value = 0.0  } } // castling move
        };

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

        /// <summary>
        /// Defines the mapping of game end conditions from PGN result strings to internal representations.
        /// </summary>
        public static readonly Dictionary<string, string> GameEndConditions = new()
        {
            { "1-0", "W" },
            { "0-1", "B" },
            { "1/2-1/2", "D" }
        };
    }
}
