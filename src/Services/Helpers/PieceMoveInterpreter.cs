using Interfaces;
using Interfaces.DTO;

namespace Services.Helpers
{
    public interface IPieceMoveInterpreter
    {
        int GetSourceSquare(BoardPosition previousBoardPosition, Ply ply);
    }

    public class PieceMoveInterpreter(ISourceSquareHelper sourceSquareHelper) : IPieceMoveInterpreter
    {
        public int GetSourceSquare(BoardPosition previousBoardPosition, Ply ply)
        {
            // Example Rhxh4, an example of a move where we need to differentiate between two rooks that can move to h4
            (int sourceRank, int sourceFile) = sourceSquareHelper.GetSourceRankAndOrFile(ply.RawMove);

            var sourceSquare = Constants.MoveNotFound;

            // must be N, B, R, Q ot K
            switch (ply.Piece.Name)
            {
                case 'N':
                    // get the eight possible squares the original N may have come from
                    //  1. DR -2, DF - 1
                    foreach (var np in Constants.RelativeKnightPositions)
                    {
                        var potentialSourceFile = ply.DestinationFile + np.file;
                        var potentialSourceRank = ply.DestinationRank + np.rank;

                        sourceSquare = sourceSquareHelper.GetSourceSquare(
                                previousBoardPosition,
                                potentialSourceRank,
                                potentialSourceFile,
                                ply.Piece,
                                ply.Colour);


                        if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                            potentialSourceRank,
                            potentialSourceFile,
                            sourceRank,
                            sourceFile) == true) break;
                    }
                    break;
                case 'B':
                    for (var diagDist = 1; diagDist < 8; diagDist++)
                    {
                        for (var dir = 0; dir < 4; dir++)
                        {
                            var fileAdj = diagDist * (2 * (dir / 2) - 1); // (dir/2)*2 - 1
                            var rankAdj = diagDist * (2 * (dir % 2) - 1); // (dir%2)*2 - 1
                            var potentialSourceFile = ply.DestinationFile + fileAdj;
                            var potentialSourceRank = ply.DestinationRank + rankAdj;

                            sourceSquare = sourceSquareHelper.GetSourceSquare(
                                previousBoardPosition,
                                potentialSourceRank,
                                potentialSourceFile,
                                ply.Piece,
                                ply.Colour);

                            if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                potentialSourceRank,
                                potentialSourceFile,
                                sourceRank,
                                sourceFile) == true) break;
                        }
                        if (sourceSquare >= 0) break;
                    }
                    break;
                case 'R':
                    for (var orthoDist = 1; orthoDist < 8; orthoDist++)
                    {
                        for (var dir = 0; dir < 4; dir++)
                        {
                            int fileAdj = orthoDist * (dir % 2) * (dir - 2); // (dir%2)*(dir-2)
                            int rankAdj = orthoDist * ((dir + 1) % 2) * (dir - 1); // ((dir+1)%2)*(dir-1)
                            var potentialSourceFile = ply.DestinationFile + fileAdj;
                            var potentialSourceRank = ply.DestinationRank + rankAdj;

                            sourceSquare = sourceSquareHelper.GetSourceSquare(
                                previousBoardPosition,
                                potentialSourceRank,
                                potentialSourceFile,
                                ply.Piece,
                                ply.Colour);

                            if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                potentialSourceRank,
                                potentialSourceFile,
                                sourceRank,
                                sourceFile) == true) break;
                        }
                        if (sourceSquare >= 0) break;
                    }
                    break;
                case 'Q':
                    // loop through diagonals (Bishop code)
                    for (var diagDist = 1; diagDist < 8; diagDist++)
                    {
                        for (var dir = 0; dir < 4; dir++)
                        {
                            var fileAdj = diagDist * (2 * (dir / 2) - 1);
                            var rankAdj = diagDist * (2 * (dir % 2) - 1);
                            var potentialSourceFile = ply.DestinationFile + fileAdj;
                            var potentialSourceRank = ply.DestinationRank + rankAdj;

                            sourceSquare = sourceSquareHelper.GetSourceSquare(
                                previousBoardPosition,
                                potentialSourceRank,
                                potentialSourceFile,
                                ply.Piece,
                                ply.Colour);

                            if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                potentialSourceRank,
                                potentialSourceFile,
                                sourceRank,
                                sourceFile) == true) break;
                        }
                        if (sourceSquare >= 0) break;
                    }
                    if (sourceSquare < 0)
                    {
                        for (var orthoDist = 1; orthoDist < 8; orthoDist++)
                        {
                            for (var dir = 0; dir < 4; dir++)
                            {
                                int fileAdj = orthoDist * (dir % 2) * (dir - 2); // (dir%2)*(dir-2)
                                int rankAdj = orthoDist * ((dir + 1) % 2) * (dir - 1); // ((dir+1)%2)*(dir-1)
                                var potentialSourceFile = ply.DestinationFile + fileAdj;
                                var potentialSourceRank = ply.DestinationRank + rankAdj;

                                sourceSquare = sourceSquareHelper.GetSourceSquare(
                                    previousBoardPosition,
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    ply.Piece,
                                    ply.Colour);

                                if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                    potentialSourceRank,
                                    potentialSourceFile,
                                    sourceRank,
                                    sourceFile) == true) break;
                            }
                            if (sourceSquare >= 0) break;
                        }
                    }


                    break;
                case 'K':
                    for (var dir = 0; dir < 4; dir++)
                    {
                        var fileAdj = (2 * (dir / 2) - 1); // (dir/2)*2 - 1
                        var rankAdj = (2 * (dir % 2) - 1); // (dir%2)*2 - 1
                        var potentialSourceFile = ply.DestinationFile + fileAdj;
                        var potentialSourceRank = ply.DestinationRank + rankAdj;

                        sourceSquare = sourceSquareHelper.GetSourceSquare(
                            previousBoardPosition,
                            potentialSourceRank,
                            potentialSourceFile,
                            ply.Piece,
                            ply.Colour);

                        if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                            potentialSourceRank,
                            potentialSourceFile,
                            sourceRank,
                            sourceFile) == true) break;
                    }
                    if (sourceSquare < 0)
                    {
                        for (var dir = 0; dir < 4; dir++)
                        {
                            int fileAdj = (dir % 2) * (dir - 2); // (dir%2)*(dir-2)
                            int rankAdj = ((dir + 1) % 2) * (dir - 1); // ((dir+1)%2)*(dir-1)
                            var potentialSourceFile = ply.DestinationFile + fileAdj;
                            var potentialSourceRank = ply.DestinationRank + rankAdj;

                            sourceSquare = sourceSquareHelper.GetSourceSquare(
                                previousBoardPosition,
                                potentialSourceRank,
                                potentialSourceFile,
                                ply.Piece,
                                ply.Colour);

                            if (sourceSquare >= 0 && RankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                potentialSourceRank,
                                potentialSourceFile,
                                sourceRank,
                                sourceFile) == true) break;
                        }
                    }
                    break;
            }

            return sourceSquare;
        }
    
    }
}
