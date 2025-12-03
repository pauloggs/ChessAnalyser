using Interfaces;
using Interfaces.DTO;
using static Interfaces.Constants;

namespace Services.Helpers
{
    public interface IBoardPositionUpdater
    {
        void UpdateCurrentBoardPositionWithMove(
            BoardPosition currentBoardPosition,
            Ply ply,
            int sourceSquare,
            int destinationSquare);
    }

    public class BoardPositionUpdater (IBitBoardManipulator bitBoardManipulator) : IBoardPositionUpdater
    {
        public void UpdateCurrentBoardPositionWithMove(
            BoardPosition currentBoardPosition,
            Ply ply,
            int sourceSquare,
            int destinationSquare)
        {
            //string piecePositionsKey = new([colour, ply.Piece]);
            string piecePositionsKey = ply.Colour.ToString() + ply.Piece.Name;

            // if it's a capture, then remove the piece from the opposite colour bitboard
            // need to find the piece! or just run through them all

            if (ply.IsPieceMove)
            {
                var piecePositions = currentBoardPosition.PiecePositions[piecePositionsKey];

                var newPiecePositions
                    = bitBoardManipulator.PiecePositionsAfterMove(piecePositions, sourceSquare, destinationSquare);

                if (ply.IsCapture)
                {
                    var oppCol = ply.Colour == Colour.W ? 'B' : 'W';

                    // update the opposing colour's piece position to remove the piece at the destination square
                    foreach (var piece in Constants.PieceIndex.Keys)
                    {

                        string oppPiecePositionsKey = new([oppCol, piece]);
                        currentBoardPosition.PiecePositions[oppPiecePositionsKey]
                            = bitBoardManipulator.RemovePiece(
                                currentBoardPosition.PiecePositions[oppPiecePositionsKey],
                                destinationSquare);
                    }

                    currentBoardPosition.PiecePositions[piecePositionsKey] = newPiecePositions;
                }

                currentBoardPosition.PiecePositions[piecePositionsKey] = newPiecePositions;
            }
            else if (ply.IsKingsideCastling)
            {
                // handle king-side castling for that particular colour
                if (ply.Colour == Colour.W)
                {
                    // move the king
                    var kingPositions = currentBoardPosition.PiecePositions["WK"];

                    currentBoardPosition.PiecePositions["WK"]
                        = bitBoardManipulator.PiecePositionsAfterMove(kingPositions, 4, 6);

                    // then move the rook
                    var rookPositions = currentBoardPosition.PiecePositions["WR"];

                    currentBoardPosition.PiecePositions["WR"]
                        = bitBoardManipulator.PiecePositionsAfterMove(rookPositions, 7, 5);
                }
                else
                {
                    // move the king
                    var kingPositions = currentBoardPosition.PiecePositions["BK"];

                    currentBoardPosition.PiecePositions["BK"]
                        = bitBoardManipulator.PiecePositionsAfterMove(kingPositions, 60, 62);

                    // then move the rook
                    var rookPositions = currentBoardPosition.PiecePositions["BR"];

                    currentBoardPosition.PiecePositions["BR"]
                        = bitBoardManipulator.PiecePositionsAfterMove(rookPositions, 63, 61);
                }
            }
            else if (ply.IsQueensideCastling)
            {
                // handle king-side castling for that particular colour
                if (ply.Colour == Colour.W)
                {
                    // move the king
                    var kingPositions = currentBoardPosition.PiecePositions["WK"];

                    currentBoardPosition.PiecePositions["WK"]
                        = bitBoardManipulator.PiecePositionsAfterMove(kingPositions, 4, 2);

                    // then move the rook
                    var rookPositions = currentBoardPosition.PiecePositions["WR"];

                    currentBoardPosition.PiecePositions["WR"]
                        = bitBoardManipulator.PiecePositionsAfterMove(rookPositions, 0, 3);
                }
                else
                {
                    // move the king
                    var kingPositions = currentBoardPosition.PiecePositions["BK"];

                    currentBoardPosition.PiecePositions["BK"]
                        = bitBoardManipulator.PiecePositionsAfterMove(kingPositions, 60, 58);

                    // then move the rook
                    var rookPositions = currentBoardPosition.PiecePositions["BR"];

                    currentBoardPosition.PiecePositions["BR"]
                        = bitBoardManipulator.PiecePositionsAfterMove(rookPositions, 56, 59);
                }
            }
            else
            {
                throw new Exception($"Move is invalid {ply.RawMove}");
            }
        }
    }
}
