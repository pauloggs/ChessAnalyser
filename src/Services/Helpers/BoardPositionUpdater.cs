using Interfaces;
using Interfaces.DTO;
using Services.Helpers.BoardUpdater;
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

    public class BoardPositionUpdater (
        IBitBoardManipulator bitBoardManipulator,
        IUpdaterEnPassant updaterEnPassant,
        IUpdaterKingsideCastling updaterKingsideCastling,
        IUpdaterNonPromotion updaterNonPromotion,
        IUpdaterPromotion updaterPromotion,
        IUpdaterQueensideCastling updaterQueensideCastling) : IBoardPositionUpdater
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

            if (ply.IsEnpassant)
            {
                updaterEnPassant.UpdateBoard(
                        currentBoardPosition,
                        piecePositionsKey,
                        ply,
                        sourceSquare,
                        destinationSquare);
            }
            else if (ply.IsPieceMove)
            {
                if (ply.IsPromotion)
                {
                    updaterPromotion.UpdateBoard(
                        currentBoardPosition,
                        piecePositionsKey,
                        ply,
                        sourceSquare,
                        destinationSquare);
                }
                else
                {
                    updaterNonPromotion.UpdateBoard(
                        currentBoardPosition,
                        piecePositionsKey,
                        ply,
                        sourceSquare,
                        destinationSquare);
                }                    
            }
            else if (ply.IsKingsideCastling)
            {
                updaterKingsideCastling.UpdateBoard(
                        currentBoardPosition,
                        piecePositionsKey,
                        ply,
                        sourceSquare,
                        destinationSquare);
            }
            else if (ply.IsQueensideCastling)
            {
                updaterQueensideCastling.UpdateBoard(
                        currentBoardPosition,
                        piecePositionsKey,
                        ply,
                        sourceSquare,
                        destinationSquare);
            }
            //else if (ply.IsEnpassant)
            //{
            //    // move the pawn
            //    var pawnPositions = currentBoardPosition.PiecePositions[piecePositionsKey];
            //    currentBoardPosition.PiecePositions[piecePositionsKey]
            //        = bitBoardManipulator.MovePiece(pawnPositions, sourceSquare, destinationSquare);

            //    // remove the captured pawn
            //    var oppCol = ply.Colour == Colour.W ? 'B' : 'W';
            //    string oppPawnPositionsKey = new([oppCol, 'P']);
            //    currentBoardPosition.PiecePositions[oppPawnPositionsKey]
            //        = bitBoardManipulator.RemovePiece(
            //            currentBoardPosition.PiecePositions[oppPawnPositionsKey],
            //            destinationSquare + (ply.Colour == Colour.W ? -8 : 8));
            //}
            else
            {
                throw new Exception($"Move is invalid {ply.RawMove}");
            }
        }
    }
}
