using Interfaces;
using Interfaces.DTO;
using Services;
using Services.Helpers;
using static Interfaces.Constants;

namespace ServicesHelpersTests
{
    /// <summary>
    /// Tests for PieceSourceFinderService that use real board positions and real dependencies.
    /// These tests would have caught the bug where knight/king source was computed as destination+delta
    /// instead of destination-delta (causing e.g. Ng3 to not find the knight on g1, and -1 to corrupt the board).
    /// </summary>
    public class PieceSourceFinderServiceTests
    {
        private static BoardPosition CreateBoardWithWhiteKnightOn( int square )
        {
            var pos = new BoardPosition();
            pos.PiecePositions["WN"] = 1UL << square;
            return pos;
        }

        private static BoardPosition CreateBoardWithWhiteKnightsOn( params int[] squares )
        {
            ulong wn = 0;
            foreach (var s in squares)
                wn |= 1UL << s;
            var pos = new BoardPosition();
            pos.PiecePositions["WN"] = wn;
            return pos;
        }

        private static BoardPosition CreateBoardWithBlackKingOn( int square )
        {
            var pos = new BoardPosition();
            pos.PiecePositions["BK"] = 1UL << square;
            return pos;
        }

        private static BoardPosition CreateBoardWithWhiteBishopOn( int square )
        {
            var pos = new BoardPosition();
            pos.PiecePositions["WB"] = 1UL << square;
            return pos;
        }

        private static BoardPosition CreateBoardWithWhiteRookOn( int square )
        {
            var pos = new BoardPosition();
            pos.PiecePositions["WR"] = 1UL << square;
            return pos;
        }

        private static BoardPosition CreateBoardWithWhiteQueenOn( int square )
        {
            var pos = new BoardPosition();
            pos.PiecePositions["WQ"] = 1UL << square;
            return pos;
        }

        [Fact(DisplayName = "FindKnightSource_Nf3_WhenKnightOnG1_ReturnsSquare6")]
        public void FindKnightSource_Nf3_WhenKnightOnG1_ReturnsSquare6()
        {
            // Valid knight move: Nf3 from g1 (square 6). Destination f3 = rank 2, file 5.
            // Source must be computed as destination - delta, not destination + delta.
            var helper = new BitBoardManipulatorHelper();
            var manipulator = new BitBoardManipulator(helper);
            var sourceSquareHelper = new SourceSquareHelper(manipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var legalMoveChecker = new LegalMoveChecker(manipulator);
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator, legalMoveChecker);

            var board = CreateBoardWithWhiteKnightOn(Squares.G1); // g1 = 6
            var ply = new Ply
            {
                RawMove = "Nf3",
                DestinationRank = 2,  // f3
                DestinationFile = 5,
                Piece = Constants.Pieces['N'],
                Colour = Colour.W
            };

            int source = sut.FindKnightSource(board, ply, -1, -1);

            Assert.Equal(Squares.G1, source);
        }

        [Fact(DisplayName = "FindKnightSource_Nce2_WhenKnightsOnC3AndG3_ReturnsC3_Square18")]
        public void FindKnightSource_Nce2_WhenKnightsOnC3AndG3_ReturnsC3_Square18()
        {
            // Scenario that was failing: Nce2 (knight on c-file to e2). Only c3 can reach e2; g3 cannot.
            // With wrong sign we would look in wrong squares and return -1.
            var helper = new BitBoardManipulatorHelper();
            var manipulator = new BitBoardManipulator(helper);
            var sourceSquareHelper = new SourceSquareHelper(manipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var legalMoveChecker = new LegalMoveChecker(manipulator);
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator, legalMoveChecker);

            int c3 = 2 * 8 + 2;  // 18
            int g3 = 2 * 8 + 6;  // 22
            var board = CreateBoardWithWhiteKnightsOn(c3, g3);
            var ply = new Ply
            {
                RawMove = "Nce2",
                DestinationRank = 1,  // e2
                DestinationFile = 4,
                Piece = Constants.Pieces['N'],
                Colour = Colour.W
            };
            int specifiedFile = 2; // 'c'
            int specifiedRank = -1;

            int source = sut.FindKnightSource(board, ply, specifiedRank, specifiedFile);

            Assert.Equal(c3, source);
        }

        [Fact(DisplayName = "FindKingSource_Kh8_WhenKingOnG8_ReturnsSquare62")]
        public void FindKingSource_Kh8_WhenKingOnG8_ReturnsSquare62()
        {
            // Scenario: 17...Kh8 with black king on g8 (62), destination h8 (63).
            // Source must be destination - (0, -1) = (7, 6) = 62.
            var helper = new BitBoardManipulatorHelper();
            var manipulator = new BitBoardManipulator(helper);
            var sourceSquareHelper = new SourceSquareHelper(manipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var legalMoveChecker = new LegalMoveChecker(manipulator);
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator, legalMoveChecker);

            var board = CreateBoardWithBlackKingOn(Squares.G8); // 62
            var ply = new Ply
            {
                RawMove = "Kh8",
                DestinationRank = 7,
                DestinationFile = 7,
                Piece = Constants.Pieces['K'],
                Colour = Colour.B
            };

            int source = sut.FindKingSource(board, ply);

            Assert.Equal(Squares.G8, source);
        }

        [Fact(DisplayName = "FindKnightSource_WhenNoKnightCanReachDestination_ReturnsMoveNotFound")]
        public void FindKnightSource_WhenNoKnightCanReachDestination_ReturnsMoveNotFound()
        {
            var helper = new BitBoardManipulatorHelper();
            var manipulator = new BitBoardManipulator(helper);
            var sourceSquareHelper = new SourceSquareHelper(manipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var legalMoveChecker = new LegalMoveChecker(manipulator);
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator, legalMoveChecker);

            var board = CreateBoardWithWhiteKnightOn(0); // knight on a1 only
            var ply = new Ply
            {
                RawMove = "Nh8",
                DestinationRank = 7,
                DestinationFile = 7, // h8 - no knight move from a1 reaches h8 in one move
                Piece = Constants.Pieces['N'],
                Colour = Colour.W
            };

            int source = sut.FindKnightSource(board, ply, -1, -1);

            Assert.Equal(MoveNotFound, source);
        }

        [Fact(DisplayName = "FindKnightSource_Ne7_WhenKnightsOnC6AndG8_AndC6PinnedByBb5_ReturnsG8")]
        public void FindKnightSource_Ne7_WhenKnightsOnC6AndG8_AndC6PinnedByBb5_ReturnsG8()
        {
            // Position after 5.Bb5: Black knights on c6 (42) and g8 (62), White bishop on b5 (33), Black king e8 (60).
            // 5...Ne7: both knights can geometrically reach e7, but c6 is pinned (moving it would leave BK in check). Only g8 is legal.
            var helper = new BitBoardManipulatorHelper();
            var manipulator = new BitBoardManipulator(helper);
            var sourceSquareHelper = new SourceSquareHelper(manipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var legalMoveChecker = new LegalMoveChecker(manipulator);
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator, legalMoveChecker);

            int c6 = 5 * 8 + 2;   // 42
            int g8 = 7 * 8 + 6;   // 62
            int b5 = 4 * 8 + 1;   // 33
            int e8 = 7 * 8 + 4;   // 60
            var board = new BoardPosition();
            board.PiecePositions["BN"] = (1UL << c6) | (1UL << g8);
            board.PiecePositions["BK"] = 1UL << e8;
            board.PiecePositions["WB"] = 1UL << b5;

            var ply = new Ply
            {
                RawMove = "Ne7",
                DestinationRank = 6,  // e7 = rank 7 in 1-based = 6 in 0-based
                DestinationFile = 4,
                Piece = Constants.Pieces['N'],
                Colour = Colour.B
            };

            int source = sut.FindKnightSource(board, ply, -1, -1);

            Assert.Equal(g8, source);  // Only the unpinned knight (g8) can move
        }

        [Fact(DisplayName = "FindBishopSource_Bf4_WhenBishopOnC1_ReturnsSquare2")]
        public void FindBishopSource_Bf4_WhenBishopOnC1_ReturnsSquare2()
        {
            var helper = new BitBoardManipulatorHelper();
            var manipulator = new BitBoardManipulator(helper);
            var sourceSquareHelper = new SourceSquareHelper(manipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var legalMoveChecker = new LegalMoveChecker(manipulator);
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator, legalMoveChecker);

            int c1 = 0 * 8 + 2;
            var board = CreateBoardWithWhiteBishopOn(c1);
            var ply = new Ply
            {
                RawMove = "Bf4",
                DestinationRank = 3,
                DestinationFile = 5,
                Piece = Constants.Pieces['B'],
                Colour = Colour.W
            };

            int source = sut.FindBishopSource(board, ply, -1, -1);

            Assert.Equal(c1, source);
        }

        [Fact(DisplayName = "FindRookSource_Ra4_WhenRookOnA1_ReturnsSquare0")]
        public void FindRookSource_Ra4_WhenRookOnA1_ReturnsSquare0()
        {
            var helper = new BitBoardManipulatorHelper();
            var manipulator = new BitBoardManipulator(helper);
            var sourceSquareHelper = new SourceSquareHelper(manipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var legalMoveChecker = new LegalMoveChecker(manipulator);
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator, legalMoveChecker);

            int a1 = 0;
            var board = CreateBoardWithWhiteRookOn(a1);
            var ply = new Ply
            {
                RawMove = "Ra4",
                DestinationRank = 3,
                DestinationFile = 0,
                Piece = Constants.Pieces['R'],
                Colour = Colour.W
            };

            int source = sut.FindRookSource(board, ply, -1, -1);

            Assert.Equal(a1, source);
        }

        [Fact(DisplayName = "FindQueenSource_Qd4_WhenQueenOnD1_ReturnsSquare3")]
        public void FindQueenSource_Qd4_WhenQueenOnD1_ReturnsSquare3()
        {
            var helper = new BitBoardManipulatorHelper();
            var manipulator = new BitBoardManipulator(helper);
            var sourceSquareHelper = new SourceSquareHelper(manipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var legalMoveChecker = new LegalMoveChecker(manipulator);
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator, legalMoveChecker);

            int d1 = 0 * 8 + 3;
            var board = CreateBoardWithWhiteQueenOn(d1);
            var ply = new Ply
            {
                RawMove = "Qd4",
                DestinationRank = 3,
                DestinationFile = 3,
                Piece = Constants.Pieces['Q'],
                Colour = Colour.W
            };

            int source = sut.FindQueenSource(board, ply, -1, -1);

            Assert.Equal(d1, source);
        }

        // ----- Determinism and ambiguous-move tests (no tie-breaking; PGN must disambiguate) -----

        [Fact(DisplayName = "FindKnightSource_WhenTwoKnightsCanLegallyReachSameSquare_NoDisambiguation_ReturnsMoveNotFound")]
        public void FindKnightSource_WhenTwoKnightsCanLegallyReachSameSquare_NoDisambiguation_ReturnsMoveNotFound()
        {
            // Two white knights on c3 and g3; both can legally move to e4. PGN "Ne4" has no file/rank → ambiguous; parser must not guess.
            var helper = new BitBoardManipulatorHelper();
            var manipulator = new BitBoardManipulator(helper);
            var sourceSquareHelper = new SourceSquareHelper(manipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var legalMoveChecker = new LegalMoveChecker(manipulator);
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator, legalMoveChecker);

            int c3 = 2 * 8 + 2;   // 18
            int g3 = 2 * 8 + 6;   // 22
            var board = CreateBoardWithWhiteKnightsOn(c3, g3);
            var ply = new Ply
            {
                RawMove = "Ne4",
                DestinationRank = 3,
                DestinationFile = 4,
                Piece = Constants.Pieces['N'],
                Colour = Colour.W
            };

            int source = sut.FindKnightSource(board, ply, -1, -1);

            Assert.Equal(MoveNotFound, source);
        }

        [Fact(DisplayName = "FindRookSource_WhenTwoRooksCanLegallyReachSameSquare_NoDisambiguation_ReturnsMoveNotFound")]
        public void FindRookSource_WhenTwoRooksCanLegallyReachSameSquare_NoDisambiguation_ReturnsMoveNotFound()
        {
            // Two white rooks on e1 and e7; both can move to e4 (no blocks). "Re4" with no disambiguation → ambiguous.
            var helper = new BitBoardManipulatorHelper();
            var manipulator = new BitBoardManipulator(helper);
            var sourceSquareHelper = new SourceSquareHelper(manipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var legalMoveChecker = new LegalMoveChecker(manipulator);
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator, legalMoveChecker);

            int e1 = 0 * 8 + 4;   // 4
            int e7 = 6 * 8 + 4;   // 52
            var board = new BoardPosition();
            board.PiecePositions["WR"] = (1UL << e1) | (1UL << e7);
            var ply = new Ply
            {
                RawMove = "Re4",
                DestinationRank = 3,
                DestinationFile = 4,
                Piece = Constants.Pieces['R'],
                Colour = Colour.W
            };

            int source = sut.FindRookSource(board, ply, -1, -1);

            Assert.Equal(MoveNotFound, source);
        }

        [Fact(DisplayName = "FindBishopSource_WhenTwoBishopsCanLegallyReachSameSquare_NoDisambiguation_ReturnsMoveNotFound")]
        public void FindBishopSource_WhenTwoBishopsCanLegallyReachSameSquare_NoDisambiguation_ReturnsMoveNotFound()
        {
            // Two white bishops on a3 and c1 (same diagonal); both attack b2. "Bb2" with no disambiguation → ambiguous.
            var helper = new BitBoardManipulatorHelper();
            var manipulator = new BitBoardManipulator(helper);
            var sourceSquareHelper = new SourceSquareHelper(manipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var legalMoveChecker = new LegalMoveChecker(manipulator);
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator, legalMoveChecker);

            int a3 = 2 * 8 + 0;   // 16
            int c1 = 0 * 8 + 2;   // 2
            var board = new BoardPosition();
            board.PiecePositions["WB"] = (1UL << a3) | (1UL << c1);
            var ply = new Ply
            {
                RawMove = "Bb2",
                DestinationRank = 1,
                DestinationFile = 1,
                Piece = Constants.Pieces['B'],
                Colour = Colour.W
            };

            int source = sut.FindBishopSource(board, ply, -1, -1);

            Assert.Equal(MoveNotFound, source);
        }

        [Fact(DisplayName = "FindQueenSource_WhenTwoQueensCanLegallyReachSameSquare_NoDisambiguation_ReturnsMoveNotFound")]
        public void FindQueenSource_WhenTwoQueensCanLegallyReachSameSquare_NoDisambiguation_ReturnsMoveNotFound()
        {
            // Two white queens on d1 and d5; both can move to d3 (clear file). "Qd3" with no disambiguation → ambiguous.
            var helper = new BitBoardManipulatorHelper();
            var manipulator = new BitBoardManipulator(helper);
            var sourceSquareHelper = new SourceSquareHelper(manipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var legalMoveChecker = new LegalMoveChecker(manipulator);
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator, legalMoveChecker);

            int d1 = 0 * 8 + 3;   // 3
            int d5 = 4 * 8 + 3;   // 35
            var board = new BoardPosition();
            board.PiecePositions["WQ"] = (1UL << d1) | (1UL << d5);
            var ply = new Ply
            {
                RawMove = "Qd3",
                DestinationRank = 2,
                DestinationFile = 3,
                Piece = Constants.Pieces['Q'],
                Colour = Colour.W
            };

            int source = sut.FindQueenSource(board, ply, -1, -1);

            Assert.Equal(MoveNotFound, source);
        }

        [Fact(DisplayName = "FindKnightSource_SameInputInvokedMultipleTimes_ReturnsSameResult_Determinism")]
        public void FindKnightSource_SameInputInvokedMultipleTimes_ReturnsSameResult_Determinism()
        {
            var helper = new BitBoardManipulatorHelper();
            var manipulator = new BitBoardManipulator(helper);
            var sourceSquareHelper = new SourceSquareHelper(manipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var legalMoveChecker = new LegalMoveChecker(manipulator);
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator, legalMoveChecker);
            var board = CreateBoardWithWhiteKnightOn(Squares.G1);
            var ply = new Ply { RawMove = "Nf3", DestinationRank = 2, DestinationFile = 5, Piece = Constants.Pieces['N'], Colour = Colour.W };

            int first = sut.FindKnightSource(board, ply, -1, -1);
            int second = sut.FindKnightSource(board, ply, -1, -1);
            int third = sut.FindKnightSource(board, ply, -1, -1);

            Assert.Equal(first, second);
            Assert.Equal(second, third);
            Assert.Equal(Squares.G1, first);
        }

        [Fact(DisplayName = "FindRookSource_SameInputInvokedMultipleTimes_ReturnsSameResult_Determinism")]
        public void FindRookSource_SameInputInvokedMultipleTimes_ReturnsSameResult_Determinism()
        {
            var helper = new BitBoardManipulatorHelper();
            var manipulator = new BitBoardManipulator(helper);
            var sourceSquareHelper = new SourceSquareHelper(manipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var legalMoveChecker = new LegalMoveChecker(manipulator);
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator, legalMoveChecker);
            var board = CreateBoardWithWhiteRookOn(0); // a1
            var ply = new Ply { RawMove = "Ra4", DestinationRank = 3, DestinationFile = 0, Piece = Constants.Pieces['R'], Colour = Colour.W };

            int first = sut.FindRookSource(board, ply, -1, -1);
            int second = sut.FindRookSource(board, ply, -1, -1);

            Assert.Equal(first, second);
            Assert.Equal(0, first);
        }

        [Fact(DisplayName = "FindKingSource_SameInputInvokedMultipleTimes_ReturnsSameResult_Determinism")]
        public void FindKingSource_SameInputInvokedMultipleTimes_ReturnsSameResult_Determinism()
        {
            var helper = new BitBoardManipulatorHelper();
            var manipulator = new BitBoardManipulator(helper);
            var sourceSquareHelper = new SourceSquareHelper(manipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var legalMoveChecker = new LegalMoveChecker(manipulator);
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator, legalMoveChecker);
            var board = CreateBoardWithBlackKingOn(Squares.G8);
            var ply = new Ply { RawMove = "Kh8", DestinationRank = 7, DestinationFile = 7, Piece = Constants.Pieces['K'], Colour = Colour.B };

            int first = sut.FindKingSource(board, ply);
            int second = sut.FindKingSource(board, ply);

            Assert.Equal(first, second);
            Assert.Equal(Squares.G8, first);
        }
    }
}
