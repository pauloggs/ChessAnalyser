namespace Interfaces.DTO
{
    /// <summary>
    /// Holds information about a single ply (half-move) in a chess game.
    /// </summary>
    public class Ply
    {
        /// <summary>
        /// The move number in the game (1-based index).
        /// </summary>
        public int MoveNumber { get; set; }

        /// <summary>
        /// The raw move notation as recorded in the game (e.g., "e4", "Nf3").
        /// </summary>
        public required string RawMove { get; set; }

        /// <summary>
        /// The colour of the player making the move ('W' for White, 'B' for Black).
        /// </summary>
        public char Colour { get; set; }

        /// <summary>
        /// Whether the move involves a piece (as opposed to a pawn move or castling).
        /// </summary>
        public bool IsPieceMove { get; set; }

        /// <summary>
        /// Is the move a pawn move.
        /// </summary>
        public bool IsPawnMove { get; set; }

        /// <summary>
        /// Whether the move is a kingside castling.
        /// </summary>
        public bool IsKingsideCastling { get; set; }

        /// <summary>
        /// Whether the move is a queenside castling.
        /// </summary>
        public bool IsQueensideCastling { get; set; }

        /// <summary>
        /// Is the move an en passant capture.
        /// </summary>
        public bool IsEnpassant { get; set; }

        /// <summary>
        /// Whether the move results in a check.
        /// </summary>
        public bool IsCheck { get; set; }

        /// <summary>
        /// Whether the move is a capture.
        /// </summary>
        public bool IsCapture { get; set; }

        /// <summary>
        /// Whether the move is a promotion.
        /// </summary>
        public bool IsPromotion { get; set; }

        /// <summary>
        /// The rank (row) of the destination square (0-7).
        /// </summary>
        public int DestinationRank { get; set; }

        /// <summary>
        /// The file (column) of the destination square (0-7).
        /// </summary>
        public int DestinationFile { get; set; }

        /// <summary>
        /// The piece involved in the move (e.g., 'P' for pawn, 'N' for knight).
        /// </summary>
        public char Piece { get; set; }
    }
}