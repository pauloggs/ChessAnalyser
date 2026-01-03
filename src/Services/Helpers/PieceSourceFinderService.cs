using Interfaces;
using Interfaces.DTO;

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
        int FindKingSource(BoardPosition previousBoardPosition, Ply ply);

    }

    public class PieceSourceFinderService(
        ISourceSquareHelper sourceSquareHelper,
        IRankAndFileHelper rankAndFileHelper) : IPieceSourceFinderService
    {
        public int FindKnightSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedSourceFile)
        {
            // The 8 possible relative "L" moves a knight could have made to reach the destination
            int[] dRank = { 2, 2, 1, 1, -1, -1, -2, -2 };
            int[] dFile = { 1, -1, 2, -2, 2, -2, 1, -1 };

            for (int i = 0; i < 8; i++)
            {
                int potRank = ply.DestinationRank + dRank[i];
                int potFile = ply.DestinationFile + dFile[i];

                // 1. Ensure the potential source is on the board
                if (potRank >= 0 && potRank <= 7 && potFile >= 0 && potFile <= 7)
                {
                    // 2. Check if the piece at this potential square is the correct Knight
                    int foundSquare = sourceSquareHelper.GetSourceSquare(
                        previousBoardPosition,
                        potRank,
                        potFile,
                        ply.Piece,
                        ply.Colour);

                    if (foundSquare >= 0)
                    {
                        // 3. Check PGN disambiguation (e.g., "Nbd2" -> specifiedSourceFile = 1)
                        if (rankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                            potRank, potFile, specifiedSourceRank, specifiedSourceFile))
                        {
                            return foundSquare;
                        }
                    }
                }
            }

            return Constants.MoveNotFound;
        }

        public int FindBishopSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedSourceFile)
        {
            // Four diagonal directions: (1,1), (1,-1), (-1,1), (-1,-1)
            int[] dRank = { 1, 1, -1, -1 };
            int[] dFile = { 1, -1, 1, -1 };

            for (int dir = 0; dir < 4; dir++)
            {
                for (int dist = 1; dist < 8; dist++)
                {
                    int potRank = ply.DestinationRank + (dRank[dir] * dist);
                    int potFile = ply.DestinationFile + (dFile[dir] * dist);

                    // 1. Check if the square is off the board
                    if (potRank < 0 || potRank > 7 || potFile < 0 || potFile > 7) break;

                    int potSquare = potRank * 8 + potFile;

                    // 2. Check for ANY piece on this square
                    var (piece, _) = BitBoardReader.ReadSquare(previousBoardPosition, potSquare);

                    if (piece is not null)
                    {
                        // 3. We hit a piece! Is it the Bishop we are looking for?
                        int foundSquare = sourceSquareHelper.GetSourceSquare(
                            previousBoardPosition, potRank, potFile, ply.Piece, ply.Colour);

                        if (foundSquare >= 0)
                        {
                            // 4. Check PGN disambiguation (e.g., "Bde3" -> specifiedSourceFile = 3)
                            if (rankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                potRank, potFile, specifiedSourceRank, specifiedSourceFile))
                            {
                                return foundSquare;
                            }
                        }

                        // IMPORTANT: Stop scanning this diagonal direction because we hit a piece.
                        // A bishop cannot "see" through or jump over any occupied square.
                        break;
                    }
                }
            }

            return Constants.MoveNotFound;
        }

        public int FindRookSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedSourceFile)
        {
            // Directions: Up, Down, Left, Right
            int[] dRank = { 1, -1, 0, 0 };
            int[] dFile = { 0, 0, -1, 1 };

            for (int dir = 0; dir < 4; dir++)
            {
                for (int dist = 1; dist < 8; dist++)
                {
                    int potRank = ply.DestinationRank + (dRank[dir] * dist);
                    int potFile = ply.DestinationFile + (dFile[dir] * dist);

                    // 1. Stay on board
                    if (potRank < 0 || potRank > 7 || potFile < 0 || potFile > 7) break;

                    int potSquare = potRank * 8 + potFile;

                    // 2. Check if square is occupied
                    var (piece,_) = BitBoardReader.ReadSquare(previousBoardPosition, potSquare);

                    if (piece is not null)
                    {
                        // 3. Is it the piece we're looking for?
                        int foundSquare = sourceSquareHelper.GetSourceSquare(
                            previousBoardPosition, potRank, potFile, ply.Piece, ply.Colour);

                        if (foundSquare >= 0)
                        {
                            // 4. Check PGN disambiguation (e.g., "Rdg1" -> specifiedSourceFile = 3)
                            if (rankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                potRank, potFile, specifiedSourceRank, specifiedSourceFile))
                            {
                                return foundSquare;
                            }
                        }

                        // Stop scanning this direction because we hit A piece (even if it's not the rook)
                        break;
                    }
                }
            }
            return Constants.MoveNotFound;
        }

        public int FindQueenSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedSourceFile)
        {
            // 1. Search for a Queen on the vertical/horizontal lines (like a Rook)
            var sourceSquare = FindRookSource(previousBoardPosition, ply, specifiedSourceRank, specifiedSourceFile);

            // 2. If no Queen was found on a straight line, search diagonals (like a Bishop)
            if (sourceSquare == Constants.MoveNotFound)
            {
                sourceSquare = FindBishopSource(previousBoardPosition, ply, specifiedSourceRank, specifiedSourceFile);
            }

            return sourceSquare;
        }

        public int FindKingSource(BoardPosition previousBoardPosition, Ply ply)
        {
            // The 8 squares immediately surrounding the destination
            int[] dRank = { 1, 1, 1, 0, 0, -1, -1, -1 };
            int[] dFile = { 1, 0, -1, 1, -1, 1, 0, -1 };

            for (int i = 0; i < 8; i++)
            {
                int potRank = ply.DestinationRank + dRank[i];
                int potFile = ply.DestinationFile + dFile[i];

                // 1. Ensure the potential source is on the board
                if (potRank >= 0 && potRank <= 7 && potFile >= 0 && potFile <= 7)
                {
                    // 2. Check if the piece at this potential square is the King of the correct color
                    int foundSquare = sourceSquareHelper.GetSourceSquare(
                        previousBoardPosition,
                        potRank,
                        potFile,
                        ply.Piece,
                        ply.Colour);

                    if (foundSquare >= 0)
                    {
                        // King moves are never ambiguous in PGN (only one king exists), 
                        // so we don't need specifiedSourceRank/File checks here.
                        return foundSquare;
                    }
                }
            }

            return Constants.MoveNotFound;
        }
    }
}
