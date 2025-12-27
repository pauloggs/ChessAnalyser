using Interfaces.DTO;
using static Interfaces.Constants;

namespace Services.Helpers.BoardUpdater
{
    public interface IUpdaterQueensideCastling : IBoardUpdater
    {
    }

    public class UpdaterQueensideCastling(IBitBoardManipulator bitBoardManipulator) : IUpdaterQueensideCastling
    {
        // White Castling
        public const int WhiteKingSource = Squares.E1;
        public const int WhiteKingDestination = Squares.C1;
        public const int WhiteRookSource = Squares.A1;
        public const int WhiteRookDestination = Squares.D1;

        // Black Castling
        public const int BlackKingSource = Squares.E8;
        public const int BlackKingDestination = Squares.C8;
        public const int BlackRookSource = Squares.A8;
        public const int BlackRookDestination = Squares.D8;

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
                    = bitBoardManipulator.MovePiece(
                        kingPositions,
                        WhiteKingSource,
                        WhiteKingDestination);

                // then move the rook
                var rookPositions = currentBoardPosition.PiecePositions["WR"];

                currentBoardPosition.PiecePositions["WR"]
                    = bitBoardManipulator.MovePiece(
                        rookPositions,
                        WhiteRookSource,
                        WhiteRookDestination);
            }
            else
            {
                // move the king
                var kingPositions = currentBoardPosition.PiecePositions["BK"];

                currentBoardPosition.PiecePositions["BK"]
                    = bitBoardManipulator.MovePiece(
                        kingPositions, 
                        BlackKingSource, 
                        BlackKingDestination);

                // then move the rook
                var rookPositions = currentBoardPosition.PiecePositions["BR"];

                currentBoardPosition.PiecePositions["BR"]
                    = bitBoardManipulator.MovePiece(
                        rookPositions, 
                        BlackRookSource, 
                        BlackRookDestination);
            }
        }
    }
}
