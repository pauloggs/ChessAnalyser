using System.Text;
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
        /// Sets the board position for the current ply, based on the previous board position and the move made in the ply.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="previousBoardPosition"></param>
        /// <param name="ply"></param>
        /// <param name="currentBoardIndex"></param>
        void SetBoardPositionFromPly(
            Game game,
            BoardPosition previousBoardPosition,
            Ply ply,
            int currentBoardIndex);

        ///// <summary>
        ///// Removes a piece from the board position at the specified file and rank.
        ///// </summary>
        ///// <param name="boardPosition"></param>
        ///// <param name="piece"></param>
        ///// <param name="col"></param>
        ///// <param name="file"></param>
        ///// <param name="rank"></param>
        //void RemovePieceFromBoardPosition(BoardPosition boardPosition, char piece, int col, char file, int rank);

        ///// <summary>
        ///// Adds a piece to the board position at the specified file and rank.
        ///// </summary>
        ///// <param name="boardPosition"></param>
        ///// <param name="piece"></param>
        ///// <param name="col"></param>
        ///// <param name="file"></param>
        ///// <param name="rank"></param>
        //void AddPieceFromBoardPosition(BoardPosition boardPosition, char piece, int col, char file, int rank);

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
        IBoardPositionUpdater boardPositionUpdater,
        IBitBoardManipulator bitBoardManipulator) : IBoardPositionsHelper
    {
        private readonly IMoveInterpreter _moveInterpreter = moveInterpreter;

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
       
        //public void RemovePieceFromBoardPosition(BoardPosition boardPosition, char piece, int col, char file, int rank)
        //{
        //    // calculate the square bitboard
        //    var square = (ulong)Math.Pow(2, rank * 8 + Constants.File[file]);

        //    // determine the colour
        //    var colour = col == 0 ? "W" : "P";

        //    // remove the piece from the bitboard
        //    boardPosition.PiecePositions[colour + piece] &= ~square;
        //}

        //public void AddPieceFromBoardPosition(BoardPosition boardPosition, char piece, int col, char file, int rank)
        //{
        //    // calculate the square bitboard
        //    var square = (ulong)Math.Pow(2, rank * 8 + Constants.File[file]);

        //    // determine the colour
        //    var colour = col == 0 ? "W" : "P";

        //    // add the piece to the bitboard
        //    boardPosition.PiecePositions[colour + piece] |= square;
        //}

        public void SetBoardPositionFromPly(
            Game game,
            BoardPosition previousBoardPosition,
            Ply ply,
            int currentBoardIndex)
        {
            var currentBoardPositions
                = previousBoardPosition.DeepCopy() ?? new BoardPosition();

            game.BoardPositions[currentBoardIndex] = currentBoardPositions;

            var colour = ply.Colour;

            var (piece, sourceSquare, destinationSquare)
                    = _moveInterpreter.GetSourceAndDestinationSquares(
                        previousBoardPosition,
                        ply,
                        colour);

            boardPositionUpdater.UpdateCurrentBoardPositionWithMove(
                currentBoardPositions,
                ply,
                sourceSquare,
                destinationSquare,
                colour
                );

            Console.WriteLine($"\nBoardPositionHelper > SetBoardPositionFromPly: move {ply.MoveNumber}, {colour}, {ply.RawMove}");
            _displayService.DisplayBoardPosition(currentBoardPositions);
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

