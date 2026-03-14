using Interfaces;
using Interfaces.DTO;

namespace Services.Helpers
{
	public interface IBoardPositionsHelper
	{
        /// <summary>
        /// Gets the starting board position, with all pieces in their starting positions.
        /// This is a bitboard representation.
        /// </summary>
        /// <returns></returns>
		BoardPosition GetStartingBoardPosition();

        /// <summary>
        /// Gets the next board position from the game and ply index. 
        /// If the ply index is 0, the board position is based on the starting position.
        /// If the ply index is greater than 0, the board position is based on the 
        /// previous board position and the move made in the ply.
        /// </summary>
        BoardPosition GetBoardPositionForPly(
            Game game,
            int plyIndex);

        ///// <summary>
        ///// Sets the board position for the current ply, based on the previous board position and the move made in the ply.
        ///// Gets the next board position from the previous board position and the ply.
        ///// </summary>
        //void SetNextBoardPositionFromPly(
        //    Game game,
        //    BoardPosition previousBoardPosition,
        //    Ply ply,
        //    int currentBoardIndex);

        /// <summary>
        /// Checks if the last move resulted in a game end condition and sets the winner accordingly.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="plyIndex"></param>
        /// <returns></returns>
        bool SetWinner(Game game, int plyIndex);
    }

    public class BoardPositionsHelper(
        IMoveInterpreter moveInterpreter,
        IDisplayService displayService,
        IBoardPositionCalculator boardPositionCalculator) : IBoardPositionsHelper
    {
        private readonly IMoveInterpreter moveInterpreter = moveInterpreter;

        private readonly IDisplayService _displayService = displayService;
       
        public BoardPosition GetStartingBoardPosition()
        {
            var startingBoardPosition = new BoardPosition();

            // set white pieces
            startingBoardPosition.PiecePositions["WP"]
                = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_0000_0000;
            startingBoardPosition.PiecePositions["WN"]
                = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0100_0010;
            startingBoardPosition.PiecePositions["WB"]
                = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0010_0100;
            startingBoardPosition.PiecePositions["WR"]
                = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1000_0001;
            startingBoardPosition.PiecePositions["WQ"]
                = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1000;
            startingBoardPosition.PiecePositions["WK"]
                = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0001_0000;

            //// TODO. Remove test removal
            //RemovePieceFromBoardPosition(startingBoardPosition, 'P', 0, 'b', 1);

            // set black pieces 
            startingBoardPosition.PiecePositions["BP"]
                //   8         7         6         5         4         3         2         1
                = 0b_0000_0000_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            startingBoardPosition.PiecePositions["BN"]
                = 0b_0100_0010_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            startingBoardPosition.PiecePositions["BB"]
                = 0b_0010_0100_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            startingBoardPosition.PiecePositions["BR"]
                = 0b_1000_0001_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            startingBoardPosition.PiecePositions["BQ"]
                = 0b_0000_1000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            startingBoardPosition.PiecePositions["BK"]
                = 0b_0001_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;

            return startingBoardPosition;
        }

        /// <summary>
        /// Gets the next board position from the game and ply index.
        /// Ply zero is the first move, so BoardPositin[0] is the position after the first move.
        /// </summary>
        public BoardPosition GetBoardPositionForPly(
            Game game,
            int plyIndex)
        {
            if (game is null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            BoardPosition previousBoardPosition;

            if (plyIndex == 0)
            {
                previousBoardPosition = game.InitialBoardPosition
                    ?? throw new InvalidOperationException("InitialBoardPosition must be set before computing ply 0.");
            }
            else
            {
                if (!game.BoardPositions.TryGetValue(plyIndex - 1, out previousBoardPosition!))
                {
                    throw new InvalidOperationException($"Previous board position for ply {plyIndex - 1} is missing.");
                }
            }

            var ply = game.Plies[plyIndex];

            // Get the piece, source square, and destination square for the move
            var (piece, sourceSquare, destinationSquare)
                    = moveInterpreter.GetSourceAndDestinationSquares(
                        previousBoardPosition,
                        ply);

            var parsingContext = BuildParsingContext(game, plyIndex);

            if (sourceSquare < 0 && ply.IsPieceMove && destinationSquare >= 0)
            {
                string message = piece.Name == 'K'
                    ? "King move: no king found adjacent to destination, or move would leave the king in check (illegal)."
                    : $"Ambiguous move: multiple {piece.Name}s can reach the same square; PGN must disambiguate (e.g. by file Nce4 or rank N4e4).";
                throw new InvalidOperationException(
                    (string.IsNullOrEmpty(parsingContext) ? message : $"{message} ({parsingContext})"));
            }

            // Update the ply with the piece and squares
            ply.Piece = piece;
            ply.SourceSquare = sourceSquare;
            ply.DestinationSquare = destinationSquare;

            // Get and return the new board position after applying the move
            return boardPositionCalculator.GetBoardPositionFromPly(
                previousBoardPosition,
                ply,
                parsingContext);
        }

        private static string? BuildParsingContext(Game game, int plyIndex)
        {
            if (game == null) return null;
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(game.SourcePgnFileName))
                parts.Add($"PGN file: {game.SourcePgnFileName}");
            if (game.GameIndexInFile.HasValue)
                parts.Add($"Game #{game.GameIndexInFile}");
            if (!string.IsNullOrEmpty(game.Name))
                parts.Add($"\"{game.Name}\"");
            var moveText = game.Plies.TryGetValue(plyIndex, out var p) ? p.RawMove : "?";
            parts.Add($"Ply {plyIndex} (move {moveText})");
            return parts.Count > 0 ? string.Join(", ", parts) : null;
        }

        public bool SetWinner(Game game, int plyIndex)
        {
            // Throw exception if there are no plies
            if (game == null || game.Plies == null || game.Plies.Count == 0)
            {
                throw new ArgumentNullException(nameof(game), "Plies must have entries.");
            }

            // Throw exception if plyIndex is out of range
            if (plyIndex < 0 || plyIndex >= game.Plies.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(plyIndex), "Ply index is out of range.");
            }

            if (Constants.GameEndConditions.TryGetValue(game.Plies[plyIndex].RawMove, out var winner))
            {
                game.Winner = winner;
                return true;
            }

            return false;
        }
    }
}

