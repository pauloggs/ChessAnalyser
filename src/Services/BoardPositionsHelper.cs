using System.Text;
using Interfaces;
using Interfaces.DTO;

namespace Services
{
	public interface IBoardPositionsHelper
	{
		BoardPosition GetStartingBoardPosition();

        void SetBoardPositions(Game game);

        void RemovePieceFromBoardPosition(BoardPosition boardPosition, char piece, int col, char file, int rank);

        void AddPieceFromBoardPosition(BoardPosition boardPosition, char piece, int col, char file, int rank);
    }

    public class BoardPositionsHelper(
        IMoveInterpreter moveInterpreter,
        IDisplayService displayService,
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

        public void RemovePieceFromBoardPosition(BoardPosition boardPosition, char piece, int col, char file, int rank)
        {
            var square = (ulong)Math.Pow(2, rank * 8 + Constants.File[file]);

            var colour = col == 0 ? "W" : "P";

            boardPosition.PiecePositions[colour + piece] &= ~square;
        }

        public void AddPieceFromBoardPosition(BoardPosition boardPosition, char piece, int col, char file, int rank)
        {
            var square = (ulong)Math.Pow(2, rank * 8 + Constants.File[file]);

            var colour = col == 0 ? "W" : "P";

            boardPosition.PiecePositions[colour + piece] |= square;
        }

        public void SetBoardPositions(Game game)
        {
            var numberOfPlies = game.Plies.Keys.Count;

            for (var plyIndex = 0; plyIndex < numberOfPlies; plyIndex++)
            {
                // ply 0 is applied to create board 1
                var currentBoardIndex = plyIndex + 1;

                var previousBoardPosition = game.BoardPositions[plyIndex];

                SetBoardPositionFromPly(game, previousBoardPosition, game.Plies[plyIndex], currentBoardIndex);
            }
        }

        private void SetBoardPositionFromPly(
            Game game,
            BoardPosition previousBoardPosition,
            Ply ply,
            int currentBoardIndex)
        {
            Console.WriteLine($"BoardPositionsHelper > SetBoardPositionFromPly: rawMove {ply.RawMove}");

            var currentBoardPositions
                = ExtensionMethods.DeepCopy(previousBoardPosition) ?? new BoardPosition();

            game.BoardPositions[currentBoardIndex] = currentBoardPositions;

            var colour = ply.Colour;

            var (piece, sourceSquare, destinationSquare)
                    = _moveInterpreter.GetSourceAndDestinationSquares(
                        previousBoardPosition,
                        ply,
                        colour);

            UpdateCurrentBoardPositionWithMove(
                currentBoardPositions,
                piece,
                ply,
                sourceSquare,
                destinationSquare,
                colour
                );

            _displayService.DisplayBoardPosition(currentBoardPositions);
        }

        /// <summary>
        /// Updates the board position with the 
        /// </summary>
        /// <param name="currentBoardPosition"></param>
        /// <param name="piece"></param>
        /// <param name="sourceSquare"></param>
        /// <param name="destinationSquare"></param>
        /// <param name="colour"></param>
        private void UpdateCurrentBoardPositionWithMove(
            BoardPosition currentBoardPosition,
            Piece piece,
            Ply ply,
            int sourceSquare,
            int destinationSquare,
            char colour)
        {
            Console.WriteLine($"BoardPositionHelper > UpdateCurrentBoardPositionWithMove '{ply.RawMove}'");

            string piecePositionsKey = new(new[] { colour, ply.Piece });

            

            // if it's a capture, then remove the piece from the opposite colour bitboard
            // need to find the piece! or just run through them all

            if (ply.IsPieceMove)
            {
                var piecePositions = currentBoardPosition.PiecePositions[piecePositionsKey];

                var newPiecePositions
                    = bitBoardManipulator.PiecePositionsAfterMove(piecePositions, sourceSquare, destinationSquare);

                currentBoardPosition.PiecePositions[piecePositionsKey] = newPiecePositions;
            }
            else if (ply.IsKingsideCastling)
            {
                // handle king-side castling for that particular colour
            }
            else if (ply.IsQueensideCastling)
            {
                // handle queen-side castling for that particular colour
            }
            else
            {
                throw new Exception($"Move is invalid {ply.RawMove}");
            }
        }
    }
}

