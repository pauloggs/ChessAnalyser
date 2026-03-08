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
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator);

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
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator);

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
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator);

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
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator);

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

        [Fact(DisplayName = "FindBishopSource_Bf4_WhenBishopOnC1_ReturnsSquare2")]
        public void FindBishopSource_Bf4_WhenBishopOnC1_ReturnsSquare2()
        {
            var helper = new BitBoardManipulatorHelper();
            var manipulator = new BitBoardManipulator(helper);
            var sourceSquareHelper = new SourceSquareHelper(manipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator);

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
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator);

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
            var sut = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, manipulator);

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
    }
}
