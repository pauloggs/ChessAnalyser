using System.Numerics;
using Interfaces;
using Interfaces.DTO;
using static Interfaces.Constants;

namespace Services.Helpers
{
    /// <summary>
    /// Checks whether a move is legal (e.g. does not leave the moving side's king in check).
    /// Used to resolve ambiguous PGN moves when only one candidate is legal (e.g. pinned piece).
    /// </summary>
    public interface ILegalMoveChecker
    {
        /// <summary>
        /// Returns true if the king of the given colour is under attack in the given position.
        /// </summary>
        bool IsKingInCheck(BoardPosition position, Colour kingColour);

        /// <summary>
        /// Returns true if making the given piece move (source to destination) would leave
        /// the moving side's king in check. Uses a cloned position so the original is not modified.
        /// Handles captures: if the destination has an opponent piece, it is removed in the clone.
        /// </summary>
        bool WouldMoveLeaveKingInCheck(
            BoardPosition position,
            Colour movingSide,
            string piecePositionsKey,
            int sourceSquare,
            int destinationSquare);
    }

    public class LegalMoveChecker(IBitBoardManipulator bitBoardManipulator) : ILegalMoveChecker
    {
        private static readonly int[] KnightDRank = { 2, 2, 1, 1, -1, -1, -2, -2 };
        private static readonly int[] KnightDFile = { 1, -1, 2, -2, 2, -2, 1, -1 };

        private static readonly int[] KingDRank = { 1, 1, 1, 0, 0, -1, -1, -1 };
        private static readonly int[] KingDFile = { -1, 0, 1, -1, 1, -1, 0, 1 };

        public bool IsKingInCheck(BoardPosition position, Colour kingColour)
        {
            string kingKey = kingColour == Colour.W ? "WK" : "BK";
            if (!position.PiecePositions.TryGetValue(kingKey, out ulong kingBb) || kingBb == 0)
                return false;
            int kingSquare = BitOperations.TrailingZeroCount(kingBb);
            if (kingSquare > 63) return false; // no king
            int kr = kingSquare / 8;
            int kf = kingSquare % 8;

            Colour opponent = kingColour == Colour.W ? Colour.B : Colour.W;
            string oppPrefix = opponent == Colour.W ? "W" : "B";

            // Knight
            if (IsSquareAttackedByKnight(position, kingSquare, kr, kf, oppPrefix + "N"))
                return true;
            // King
            if (IsSquareAttackedByKing(position, kr, kf, oppPrefix + "K"))
                return true;
            // Pawn
            if (IsSquareAttackedByPawn(position, kr, kf, opponent))
                return true;

            ulong occupancy = GetAllPiecesBitboard(position);
            // Rook / Queen (rank and file)
            if (IsSquareAttackedByRookOrQueen(position, kingSquare, kr, kf, oppPrefix, occupancy))
                return true;
            // Bishop / Queen (diagonals)
            if (IsSquareAttackedByBishopOrQueen(position, kingSquare, kr, kf, oppPrefix, occupancy))
                return true;

            return false;
        }

        public bool WouldMoveLeaveKingInCheck(
            BoardPosition position,
            Colour movingSide,
            string piecePositionsKey,
            int sourceSquare,
            int destinationSquare)
        {
            var clone = position.DeepCopy();
            // Move our piece
            var pieceBb = clone.PiecePositions[piecePositionsKey];
            pieceBb = bitBoardManipulator.MovePiece(pieceBb, sourceSquare, destinationSquare);
            clone.PiecePositions[piecePositionsKey] = pieceBb;
            // Remove captured piece if any (destination had opponent piece)
            string oppPrefix = movingSide == Colour.W ? "B" : "W";
            foreach (var key in new[] { oppPrefix + "P", oppPrefix + "N", oppPrefix + "B", oppPrefix + "R", oppPrefix + "Q", oppPrefix + "K" })
            {
                if (clone.PiecePositions.TryGetValue(key, out ulong oppBb) && (oppBb & (1UL << destinationSquare)) != 0)
                {
                    clone.PiecePositions[key] = bitBoardManipulator.RemovePiece(oppBb, destinationSquare);
                    break;
                }
            }
            return IsKingInCheck(clone, movingSide);
        }

        private static ulong GetAllPiecesBitboard(BoardPosition position)
        {
            ulong all = 0;
            foreach (var kv in position.PiecePositions)
                all |= kv.Value;
            return all;
        }

        private static bool IsSquareAttackedByKnight(BoardPosition position, int targetSquare, int tr, int tf, string knightKey)
        {
            if (!position.PiecePositions.TryGetValue(knightKey, out ulong knightBb) || knightBb == 0)
                return false;
            for (int i = 0; i < 8; i++)
            {
                int pr = tr - KnightDRank[i];
                int pf = tf - KnightDFile[i];
                if (pr >= 0 && pr <= 7 && pf >= 0 && pf <= 7)
                {
                    int sq = pr * 8 + pf;
                    if ((knightBb & (1UL << sq)) != 0) return true;
                }
            }
            return false;
        }

        private static bool IsSquareAttackedByKing(BoardPosition position, int tr, int tf, string kingKey)
        {
            if (!position.PiecePositions.TryGetValue(kingKey, out ulong kingBb) || kingBb == 0)
                return false;
            for (int i = 0; i < 8; i++)
            {
                int pr = tr + KingDRank[i];
                int pf = tf + KingDFile[i];
                if (pr >= 0 && pr <= 7 && pf >= 0 && pf <= 7)
                {
                    if ((kingBb & (1UL << (pr * 8 + pf))) != 0) return true;
                }
            }
            return false;
        }

        private static bool IsSquareAttackedByPawn(BoardPosition position, int tr, int tf, Colour attackerColour)
        {
            string pawnKey = attackerColour == Colour.W ? "WP" : "BP";
            if (!position.PiecePositions.TryGetValue(pawnKey, out ulong pawnBb) || pawnBb == 0)
                return false;
            // White pawns attack (tr-1, tf-1) and (tr-1, tf+1). Black pawns attack (tr+1, tf-1), (tr+1, tf+1).
            int dr = attackerColour == Colour.W ? -1 : 1;
            int pr1 = tr - dr;
            if (pr1 >= 0 && pr1 <= 7)
            {
                if (tf - 1 >= 0 && (pawnBb & (1UL << (pr1 * 8 + (tf - 1)))) != 0) return true;
                if (tf + 1 <= 7 && (pawnBb & (1UL << (pr1 * 8 + (tf + 1)))) != 0) return true;
            }
            return false;
        }

        private static bool IsSquareAttackedByRookOrQueen(BoardPosition position, int targetSquare, int tr, int tf, string oppPrefix, ulong occupancy)
        {
            foreach (var key in new[] { oppPrefix + "R", oppPrefix + "Q" })
            {
                if (!position.PiecePositions.TryGetValue(key, out ulong bb) || bb == 0) continue;
                ulong attacks = GetRookAttacks(targetSquare, occupancy);
                if ((bb & attacks) != 0) return true;
            }
            return false;
        }

        private static bool IsSquareAttackedByBishopOrQueen(BoardPosition position, int targetSquare, int tr, int tf, string oppPrefix, ulong occupancy)
        {
            foreach (var key in new[] { oppPrefix + "B", oppPrefix + "Q" })
            {
                if (!position.PiecePositions.TryGetValue(key, out ulong bb) || bb == 0) continue;
                ulong attacks = GetBishopAttacks(targetSquare, occupancy);
                if ((bb & attacks) != 0) return true;
            }
            return false;
        }

        private static ulong GetRookAttacks(int square, ulong occupancy)
        {
            int sr = square / 8;
            int sf = square % 8;
            ulong attacks = 0;
            for (int r = sr + 1; r <= 7; r++) { attacks |= 1UL << (r * 8 + sf); if ((occupancy & (1UL << (r * 8 + sf))) != 0) break; }
            for (int r = sr - 1; r >= 0; r--) { attacks |= 1UL << (r * 8 + sf); if ((occupancy & (1UL << (r * 8 + sf))) != 0) break; }
            for (int f = sf + 1; f <= 7; f++) { attacks |= 1UL << (sr * 8 + f); if ((occupancy & (1UL << (sr * 8 + f))) != 0) break; }
            for (int f = sf - 1; f >= 0; f--) { attacks |= 1UL << (sr * 8 + f); if ((occupancy & (1UL << (sr * 8 + f))) != 0) break; }
            return attacks;
        }

        private static ulong GetBishopAttacks(int square, ulong occupancy)
        {
            int sr = square / 8;
            int sf = square % 8;
            ulong attacks = 0;
            for (int d = 1; sr + d <= 7 && sf + d <= 7; d++) { attacks |= 1UL << ((sr + d) * 8 + (sf + d)); if ((occupancy & (1UL << ((sr + d) * 8 + (sf + d)))) != 0) break; }
            for (int d = 1; sr + d <= 7 && sf - d >= 0; d++) { attacks |= 1UL << ((sr + d) * 8 + (sf - d)); if ((occupancy & (1UL << ((sr + d) * 8 + (sf - d)))) != 0) break; }
            for (int d = 1; sr - d >= 0 && sf + d <= 7; d++) { attacks |= 1UL << ((sr - d) * 8 + (sf + d)); if ((occupancy & (1UL << ((sr - d) * 8 + (sf + d)))) != 0) break; }
            for (int d = 1; sr - d >= 0 && sf - d >= 0; d++) { attacks |= 1UL << ((sr - d) * 8 + (sf - d)); if ((occupancy & (1UL << ((sr - d) * 8 + (sf - d)))) != 0) break; }
            return attacks;
        }
    }
}
