using Interfaces.DTO;
using static Interfaces.Constants;

namespace Services.Helpers.BoardUpdater
{
    public interface IUpdaterKingsideCastling : IBoardUpdater
    {
    }

    public class UpdaterKingsideCastling(IBitBoardManipulator bitBoardManipulator) : IUpdaterKingsideCastling
    {
        // White Castling
        public const int WhiteKingSource = Squares.E1;
        public const int WhiteKingDestination = Squares.G1;
        public const int WhiteRookSource = Squares.H1;
        public const int WhiteRookDestination = Squares.F1;

        // Black Castling
        public const int BlackKingSource = Squares.E8;
        public const int BlackKingDestination = Squares.G8;
        public const int BlackRookSource = Squares.H8;
        public const int BlackRookDestination = Squares.F8;

        public void UpdateBoard(
            BoardPosition currentBoardPosition,
            string piecePositionsKey,
            Ply ply,
            int sourceSquare,
            int destinationSquare)
        {
            // handle king-side castling for that particular colour
            if (ply.Colour == Colour.W)
            {
                // move the king
                var kingPositions = currentBoardPosition.PiecePositions["WK"];

                currentBoardPosition.PiecePositions["WK"]
                    = bitBoardManipulator.PiecePositionsAfterMove(
                        kingPositions,
                        WhiteKingSource,
                        WhiteKingDestination);

                // then move the rook
                var rookPositions = currentBoardPosition.PiecePositions["WR"];

                currentBoardPosition.PiecePositions["WR"]
                    = bitBoardManipulator.PiecePositionsAfterMove(
                        rookPositions,
                        WhiteRookSource,
                        WhiteRookDestination);
            }
            else
            {
                // move the king
                var kingPositions = currentBoardPosition.PiecePositions["BK"];

                currentBoardPosition.PiecePositions["BK"]
                    = bitBoardManipulator.PiecePositionsAfterMove(
                        kingPositions,
                        BlackKingSource,
                        BlackKingDestination);

                // then move the rook
                var rookPositions = currentBoardPosition.PiecePositions["BR"];

                currentBoardPosition.PiecePositions["BR"]
                    = bitBoardManipulator.PiecePositionsAfterMove(
                        rookPositions,
                        BlackRookSource,
                        BlackRookDestination);
            }
        }
    }
}
