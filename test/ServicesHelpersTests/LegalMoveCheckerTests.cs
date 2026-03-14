using Interfaces;
using Interfaces.DTO;
using Services;
using Services.Helpers;
using static Interfaces.Constants;

namespace ServicesHelpersTests
{
    /// <summary>
    /// Tests for LegalMoveChecker, including pawn attack direction (white pawns attack upward in rank).
    /// </summary>
    public class LegalMoveCheckerTests
    {
        private static BoardPosition CreatePosition(ulong wk, ulong bk, ulong wp, ulong bp)
        {
            var pos = new BoardPosition();
            pos.PiecePositions["WK"] = wk;
            pos.PiecePositions["BK"] = bk;
            pos.PiecePositions["WP"] = wp;
            pos.PiecePositions["BP"] = bp;
            pos.PiecePositions["WN"] = pos.PiecePositions["BN"] = pos.PiecePositions["WB"] = pos.PiecePositions["BB"] = 0;
            pos.PiecePositions["WR"] = pos.PiecePositions["BR"] = pos.PiecePositions["WQ"] = pos.PiecePositions["BQ"] = 0;
            return pos;
        }

        [Fact(DisplayName = "Black_king_on_g3_with_white_pawn_on_h3_only_is_NOT_in_check")]
        public void BlackKingOnG3_WhitePawnOnH3Only_IsNotInCheck()
        {
            // Regression: white pawn on h3 attacks g4, not g3. Black king on g3 must not be considered in check.
            // g3 = 22, h3 = 23 (rank 2, files 6 and 7).
            var pos = CreatePosition(wk: 1UL << 26, bk: 1UL << 22, wp: 1UL << 23, bp: 0); // WK c4, BK g3, WP h3
            var checker = new LegalMoveChecker(new BitBoardManipulator(new BitBoardManipulatorHelper()));
            Assert.False(checker.IsKingInCheck(pos, Colour.B));
        }

        [Fact(DisplayName = "Black_king_on_g3_with_white_pawn_on_h2_IS_in_check")]
        public void BlackKingOnG3_WhitePawnOnH2_IsInCheck()
        {
            // White pawn on h2 (square 15) attacks g3 (square 22). Black king on g3 must be in check.
            var pos = CreatePosition(wk: 0, bk: 1UL << 22, wp: 1UL << 15, bp: 0);
            var checker = new LegalMoveChecker(new BitBoardManipulator(new BitBoardManipulatorHelper()));
            Assert.True(checker.IsKingInCheck(pos, Colour.B));
        }
    }
}
