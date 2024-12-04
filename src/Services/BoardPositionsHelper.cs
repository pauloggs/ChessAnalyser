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

    public class BoardPositionsHelper(IMoveInterpreter moveInterpreter) : IBoardPositionsHelper
    {
        private readonly IMoveInterpreter _moveInterpreter = moveInterpreter;

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

            for (var currentPlyKey = 0; currentPlyKey < numberOfPlies; currentPlyKey++)
            {
                var previousBoardPosition = game.BoardPositions[currentPlyKey];

                SetBoardPositionFromPly(game, previousBoardPosition, game.Plies[currentPlyKey], currentPlyKey+1);
            }
        }

        private void SetBoardPositionFromPly(
            Game game,
            BoardPosition previousBoardPosition,
            Ply ply,
            int currentBoardIndex)
        {
            var newBoardPosition
                = ExtensionMethods.DeepCopy(previousBoardPosition) ?? new BoardPosition();

            game.BoardPositions[currentBoardIndex] = newBoardPosition;

            var colour = ply.Colour;

            var (piece, sourceSquare, destinationSquare)
                    = _moveInterpreter.GetSourceAndDestinationSquares(
                        previousBoardPosition,
                        ply,
                        colour);

            UpdateCurrentBoardPositionWithMove(
                newBoardPosition,
                piece,
                ply,
                sourceSquare,
                destinationSquare,
                colour
                );
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
                var piecePositionBytes
                = BitConverter.GetBytes(currentBoardPosition.PiecePositions[piecePositionsKey])
                ?? Array.Empty<byte>();
                // handle  pawn and piece moves
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

