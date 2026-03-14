using Interfaces;
using Interfaces.DTO;
using static Interfaces.Constants;

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
        IRankAndFileHelper rankAndFileHelper,
        IBitBoardManipulator bitBoardManipulator,
        ILegalMoveChecker legalMoveChecker) : IPieceSourceFinderService
    {
        private readonly ISourceSquareHelper sourceSquareHelper = sourceSquareHelper;

        private readonly IRankAndFileHelper rankAndFileHelper = rankAndFileHelper;

        private readonly IBitBoardManipulator bitBoardManipulator = bitBoardManipulator;

        private readonly ILegalMoveChecker legalMoveChecker = legalMoveChecker;

        public int FindKnightSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedSourceFile)
        {
            // The 8 possible relative "L" moves a knight could have made to reach the destination
            int[] dRank = { 2, 2, 1, 1, -1, -1, -2, -2 };
            int[] dFile = { 1, -1, 2, -2, 2, -2, 1, -1 };

            var candidates = new List<int>();
            for (int i = 0; i < 8; i++)
            {
                int potRank = ply.DestinationRank - dRank[i];
                int potFile = ply.DestinationFile - dFile[i];

                if (potRank >= 0 && potRank <= 7 && potFile >= 0 && potFile <= 7)
                {
                    int foundSquare = sourceSquareHelper.GetSourceSquare(
                        previousBoardPosition,
                        potRank,
                        potFile,
                        ply.Piece,
                        ply.Colour);

                    if (foundSquare >= 0 &&
                        rankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                            potRank, potFile, specifiedSourceRank, specifiedSourceFile))
                    {
                        candidates.Add(foundSquare);
                    }
                }
            }

            if (candidates.Count == 0)
            {
                // Fallback: when no disambiguation, check piece bitboard directly for all 8 candidate squares
                if (specifiedSourceRank < 0 && specifiedSourceFile < 0 &&
                    previousBoardPosition.PiecePositions.TryGetValue(ply.PiecePositionsKey, out ulong knightBitboard))
                {
                    for (int i = 0; i < 8; i++)
                    {
                        int potRank = ply.DestinationRank - dRank[i];
                        int potFile = ply.DestinationFile - dFile[i];
                        if (potRank >= 0 && potRank <= 7 && potFile >= 0 && potFile <= 7)
                        {
                            int square = potRank * 8 + potFile;
                            if ((knightBitboard & (1UL << square)) != 0)
                                candidates.Add(square);
                        }
                    }
                    if (candidates.Count == 1)
                        return candidates[0];
                    if (candidates.Count == 0)
                        return Constants.MoveNotFound;
                    // Multiple knights can reach the destination: keep only legal moves (e.g. not pinned).
                    var legalFromFallback = FilterLegalPieceSources(previousBoardPosition, ply, candidates);
                    if (legalFromFallback.Count == 1)
                        return legalFromFallback[0];
                    if (legalFromFallback.Count == 0)
                        return Constants.MoveNotFound;
                    // Multiple legal candidates and no disambiguation in PGN: invalid; do not guess.
                    return Constants.MoveNotFound;
                }
                return Constants.MoveNotFound;
            }

            if (candidates.Count == 1)
                return candidates[0];

            if (candidates.Count == 0)
                return Constants.MoveNotFound;

            // Multiple knights can reach: filter by legality only.
            var legalCandidates = FilterLegalPieceSources(previousBoardPosition, ply, candidates);
            if (legalCandidates.Count == 1)
                return legalCandidates[0];
            if (legalCandidates.Count == 0)
                return Constants.MoveNotFound;
            // Multiple legal candidates with no disambiguation: PGN is ambiguous; do not guess.
            return Constants.MoveNotFound;
        }

        /// <summary>
        /// Filters piece source candidates to only those for which the move would not leave the king in check (e.g. excludes pinned pieces).
        /// Used for knights, bishops, rooks, and queens.
        /// </summary>
        private List<int> FilterLegalPieceSources(BoardPosition position, Ply ply, List<int> candidates)
        {
            int destSquare = ply.DestinationRank * 8 + ply.DestinationFile;
            var legal = new List<int>();
            foreach (int src in candidates)
            {
                if (!legalMoveChecker.WouldMoveLeaveKingInCheck(position, ply.Colour, ply.PiecePositionsKey, src, destSquare))
                    legal.Add(src);
            }
            return legal;
        }

        public int FindBishopSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedSourceFile)
        {
            var candidates = CollectBishopCandidates(previousBoardPosition, ply, specifiedSourceRank, specifiedSourceFile);
            return ResolveCandidatesWithLegalFilter(previousBoardPosition, ply, candidates);
        }

        private List<int> CollectBishopCandidates(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedSourceFile)
        {
            int[] dRank = { 1, 1, -1, -1 };
            int[] dFile = { 1, -1, 1, -1 };
            var candidates = new List<int>();

            for (int dir = 0; dir < 4; dir++)
            {
                for (int dist = 1; dist < 8; dist++)
                {
                    int potRank = ply.DestinationRank + (dRank[dir] * dist);
                    int potFile = ply.DestinationFile + (dFile[dir] * dist);
                    if (potRank < 0 || potRank > 7 || potFile < 0 || potFile > 7) break;

                    int potSquare = potRank * 8 + potFile;
                    var (piece, _) = bitBoardManipulator.ReadSquare(previousBoardPosition, potSquare);

                    if (piece is not null)
                    {
                        int foundSquare = sourceSquareHelper.GetSourceSquare(
                            previousBoardPosition, potRank, potFile, ply.Piece, ply.Colour);
                        if (foundSquare >= 0 &&
                            rankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                potRank, potFile, specifiedSourceRank, specifiedSourceFile))
                            candidates.Add(foundSquare);
                        break;
                    }
                }
            }
            return candidates;
        }

        public int FindRookSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedSourceFile)
        {
            var candidates = CollectRookCandidates(previousBoardPosition, ply, specifiedSourceRank, specifiedSourceFile);
            return ResolveCandidatesWithLegalFilter(previousBoardPosition, ply, candidates);
        }

        private List<int> CollectRookCandidates(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedSourceFile)
        {
            int[] dRank = { 1, -1, 0, 0 };
            int[] dFile = { 0, 0, -1, 1 };
            var candidates = new List<int>();

            for (int dir = 0; dir < 4; dir++)
            {
                for (int dist = 1; dist < 8; dist++)
                {
                    int potRank = ply.DestinationRank + (dRank[dir] * dist);
                    int potFile = ply.DestinationFile + (dFile[dir] * dist);
                    if (potRank < 0 || potRank > 7 || potFile < 0 || potFile > 7) break;

                    int potSquare = potRank * 8 + potFile;
                    var (piece, _) = bitBoardManipulator.ReadSquare(previousBoardPosition, potSquare);

                    if (piece is not null)
                    {
                        int foundSquare = sourceSquareHelper.GetSourceSquare(
                            previousBoardPosition, potRank, potFile, ply.Piece, ply.Colour);
                        if (foundSquare >= 0 &&
                            rankAndFileHelper.PotentialRankOrFileMatchesSpecifiedRankOrFile(
                                potRank, potFile, specifiedSourceRank, specifiedSourceFile))
                            candidates.Add(foundSquare);
                        break;
                    }
                }
            }
            return candidates;
        }

        /// <summary>
        /// Resolves source from candidates: filter by legality only. Exactly one legal candidate → return it;
        /// zero legal → illegal move; multiple legal with no disambiguation → ambiguous (return MoveNotFound).
        /// </summary>
        private int ResolveCandidatesWithLegalFilter(BoardPosition position, Ply ply, List<int> candidates)
        {
            if (candidates.Count == 0) return Constants.MoveNotFound;
            var legal = FilterLegalPieceSources(position, ply, candidates);
            if (legal.Count == 0) return Constants.MoveNotFound;
            if (legal.Count == 1) return legal[0];
            // Multiple legal candidates and PGN did not disambiguate: invalid.
            return Constants.MoveNotFound;
        }

        public int FindQueenSource(BoardPosition previousBoardPosition, Ply ply, int specifiedSourceRank, int specifiedSourceFile)
        {
            var rookCandidates = CollectRookCandidates(previousBoardPosition, ply, specifiedSourceRank, specifiedSourceFile);
            var bishopCandidates = CollectBishopCandidates(previousBoardPosition, ply, specifiedSourceRank, specifiedSourceFile);
            var all = new List<int>(rookCandidates.Count + bishopCandidates.Count);
            all.AddRange(rookCandidates);
            all.AddRange(bishopCandidates);
            return ResolveCandidatesWithLegalFilter(previousBoardPosition, ply, all);
        }

        public int FindKingSource(BoardPosition previousBoardPosition, Ply ply)
        {
            int[] dRank = { 1, 1, 1, 0, 0, -1, -1, -1 };
            int[] dFile = { 1, 0, -1, 1, -1, 1, 0, -1 };
            for (int i = 0; i < 8; i++)
            {
                int potRank = ply.DestinationRank - dRank[i];
                int potFile = ply.DestinationFile - dFile[i];
                if (potRank >= 0 && potRank <= 7 && potFile >= 0 && potFile <= 7)
                {
                    int foundSquare = sourceSquareHelper.GetSourceSquare(
                        previousBoardPosition, potRank, potFile, ply.Piece, ply.Colour);
                    if (foundSquare >= 0)
                    {
                        // Only one king per side; return it. Legality (moving into check) is enforced in GetBoardPositionFromNonPromotion.
                        return foundSquare;
                    }
                }
            }
            return Constants.MoveNotFound;
        }
    }
}
