using Interfaces;
using Interfaces.DTO;
using System.Diagnostics;
namespace Services.Helpers
{
    public interface IPieceSourceFinderService
    {
        /// <summary>
        /// Iterates through all potential knight source squares relative to the move's destination, returning 
        /// the 0-63 index of the first valid square found that matches board constraints and any specified 
        /// rank/file disambiguation provided in the PGN Ply object.
        /// </summary>
        int FindKnightSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedourceFile);

        /// <summary>
        /// Iterates through all potential bishop source squares relative to the move's destination, returning 
        /// the 0-63 index of the first valid square found that matches board constraints and any specified 
        /// rank/file disambiguation provided in the PGN Ply object.
        /// </summary>
        int FindBishopSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedourceFile);

        /// <summary>
        /// Iterates through all potential rook source squares relative to the move's destination, returning 
        /// the 0-63 index of the first valid square found that matches board constraints and any specified 
        /// rank/file disambiguation provided in the PGN Ply object.
        /// </summary>
        int FindRookSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedourceFile);

        /// <summary>
        /// Iterates through all potential queen source squares relative to the move's destination, returning 
        /// the 0-63 index of the first valid square found that matches board constraints and any specified 
        /// rank/file disambiguation provided in the PGN Ply object.
        /// </summary>
        int FindQueenSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedourceFile);

        /// <summary>
        /// Iterates through all potential king source squares relative to the move's destination, returning 
        /// the 0-63 index of the first valid square found that matches board constraints and any specified 
        /// rank/file disambiguation provided in the PGN Ply object.
        /// </summary>
        int FindKingSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedourceFile);

    }

    public class PieceSourceFinderService(
        ISourceSquareHelper sourceSquareHelper,
        IRankAndFileHelper rankAndFileHelper) : IPieceSourceFinderService
    {
        public int FindKnightSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedourceFile)
        {
            foreach (var np in Constants.RelativeKnightPositions)
            {
                var potentialSourceFile = ply.DestinationFile + np.file;
                var potentialSourceRank = ply.DestinationRank + np.rank;

                var sourceSquare = sourceSquareHelper.GetSourceSquare(
                        previousBoardPosition, 
                        potentialSourceRank, 
                        potentialSourceFile, 
                        ply.Piece, 
                        ply.Colour);

                if (sourceSquare >= 0 && rankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                    potentialSourceRank, 
                    potentialSourceFile, 
                    specifiedSourceRank, 
                    specifiedourceFile))
                {
                    return sourceSquare;
                }
            }
            return Constants.MoveNotFound;
        }

        public int FindBishopSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedSourceFile)
        {
            var sourceSquare = Constants.MoveNotFound;

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

                    if (sourceSquare >= 0 && rankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                        potentialSourceRank,
                        potentialSourceFile,
                        specifiedSourceRank,
                        specifiedSourceFile) == true) break;
                }
                if (sourceSquare >= 0) break;
            }

            return sourceSquare;
        }

        public int FindRookSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedSourceFile)
        {
            var sourceSquare = Constants.MoveNotFound;

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

                    if (sourceSquare >= 0 && rankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                        potentialSourceRank,
                        potentialSourceFile,
                        specifiedSourceRank,
                        specifiedSourceFile) == true) break;
                }
                if (sourceSquare >= 0) break;
            }
            
            return sourceSquare;
        }

        public int FindQueenSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedSourceFile)
        {
            var sourceSquare = Constants.MoveNotFound;

            // loop through diagonal 'bishop' moves
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

                    if (sourceSquare >= 0 && rankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                        potentialSourceRank,
                        potentialSourceFile,
                        specifiedSourceRank,
                        specifiedSourceFile) == true) break;
                }
                if (sourceSquare >= 0) break;
            }
            if (sourceSquare == Constants.MoveNotFound) // instead, loop through horizontal/vertical 'rook' moves
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

                        if (sourceSquare >= 0 && rankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                            potentialSourceRank,
                            potentialSourceFile,
                            specifiedSourceRank,
                            specifiedSourceFile) == true) break;
                    }
                    if (sourceSquare >= 0) break;
                }
            }
            
            return sourceSquare;
        }

        public int FindKingSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedSourceFile)
        {
            var sourceSquare = Constants.MoveNotFound;

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

                if (sourceSquare >= 0 && rankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                    potentialSourceRank,
                    potentialSourceFile,
                    specifiedSourceRank,
                    specifiedSourceFile) == true) break;
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

                    if (sourceSquare >= 0 && rankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                        potentialSourceRank,
                        potentialSourceFile,
                        specifiedSourceRank,
                        specifiedSourceFile) == true) break;
                }
            }

            return sourceSquare;
        }
    }
}
