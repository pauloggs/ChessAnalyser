using Interfaces;
using Interfaces.DTO;

namespace Services.Helpers
{
    public interface IPieceMoveInterpreter
    {
        int GetSourceSquare(BoardPosition previousBoardPosition, Ply ply);
    }

    public class PieceMoveInterpreter(
        ISourceSquareHelper sourceSquareHelper,
        IPieceSourceFinderService pieceSourceFinderService) : IPieceMoveInterpreter
    {
        public int GetSourceSquare(BoardPosition previousBoardPosition, Ply ply)
        {
            // Example Rhxh4, an example of a move where we need to differentiate between two rooks that can move to h4
            (int sourceRank, int sourceFile) = sourceSquareHelper.GetSourceRankAndOrFile(ply.RawMove);

            var sourceSquare = Constants.MoveNotFound;

            // must be N, B, R, Q or K
            switch (ply.Piece.Name)
            {
                case 'N':
                    sourceSquare = pieceSourceFinderService.FindKnightSource(
                        previousBoardPosition,
                        ply,
                        sourceRank,
                        sourceFile);
                    break;
                case 'B':
                    sourceSquare = pieceSourceFinderService.FindBishopSource(
                        previousBoardPosition,
                        ply,
                        sourceRank,
                        sourceFile);
                    break;
                case 'R':
                    sourceSquare = pieceSourceFinderService.FindRookSource(
                        previousBoardPosition,
                        ply,
                        sourceRank,
                        sourceFile);
                    break;
                case 'Q':
                    sourceSquare = pieceSourceFinderService.FindQueenSource(
                        previousBoardPosition,
                        ply,
                        sourceRank,
                        sourceFile);
                    break;
                case 'K':
                    sourceSquare = pieceSourceFinderService.FindKingSource(
                        previousBoardPosition,
                        ply,
                        sourceRank,
                        sourceFile);
                    break;
            }

            return sourceSquare;
        }    
    }
}
