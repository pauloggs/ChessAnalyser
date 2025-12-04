using Interfaces;
using Interfaces.DTO;
namespace Services.Helpers
{
    public interface IPieceSourceFinderService
    {
        int FindKnightSource(BoardPosition bp, Ply ply, int specRank, int specFile);

        int FindBishopSource(BoardPosition bp, Ply ply, int specRank, int specFile);

        int FindRookSource(BoardPosition bp, Ply ply, int specRank, int specFile);

        int FindQueenSource(BoardPosition bp, Ply ply, int specRank, int specFile);

        int FindKingSource(BoardPosition bp, Ply ply, int specRank, int specFile);

    }

    public class PieceSourceFinderService(
        ISourceSquareHelper sourceSquareHelper,
        IRankAndFileHelper rankAndFileHelper) : IPieceSourceFinderService
    {
        public int FindKnightSource(BoardPosition bp, Ply ply, int specRank, int specFile)
        {
            foreach (var np in Constants.RelativeKnightPositions)
            {
                var potentialSourceFile = ply.DestinationFile + np.file;
                var potentialSourceRank = ply.DestinationRank + np.rank;

                var sourceSquare = sourceSquareHelper.GetSourceSquare(
                        bp, potentialSourceRank, potentialSourceFile, ply.Piece, ply.Colour);

                if (sourceSquare >= 0 && rankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                    potentialSourceRank, potentialSourceFile, specRank, specFile))
                {
                    return sourceSquare;
                }
            }
            return Constants.MoveNotFound;
        }

        public int FindBishopSource(BoardPosition bp, Ply ply, int specRank, int specFile)
        {
            throw new NotImplementedException();
        }

        public int FindRookSource(BoardPosition bp, Ply ply, int specRank, int specFile)
        {
            throw new NotImplementedException();
        }

        public int FindQueenSource(BoardPosition bp, Ply ply, int specRank, int specFile)
        {
            throw new NotImplementedException();
        }

        public int FindKingSource(BoardPosition bp, Ply ply, int specRank, int specFile)
        {
            throw new NotImplementedException();
        }
    }
}
