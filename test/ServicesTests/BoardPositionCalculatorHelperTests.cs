using Interfaces;
using Interfaces.DTO;
using Services;
using Services.Helpers;
using static Interfaces.Constants;

namespace ServicesTests
{
    public class BoardPositionCalculatorHelperTests
    {
        private static BoardPosition GetStartingPosition()
        {
            var b = new BoardPosition();
            b.PiecePositions["WP"] = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_0000_0000;
            b.PiecePositions["WN"] = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0100_0010;
            b.PiecePositions["WB"] = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0010_0100;
            b.PiecePositions["WR"] = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1000_0001;
            b.PiecePositions["WQ"] = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1000;
            b.PiecePositions["WK"] = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0001_0000;
            b.PiecePositions["BP"] = 0b_0000_0000_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            b.PiecePositions["BN"] = 0b_0100_0010_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            b.PiecePositions["BB"] = 0b_0010_0100_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            b.PiecePositions["BR"] = 0b_1000_0001_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            b.PiecePositions["BQ"] = 0b_0000_1000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            b.PiecePositions["BK"] = 0b_0001_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            return b;
        }

        private readonly IBoardPositionCalculatorHelper _sut = new BoardPositionCalculatorHelper(new BitBoardManipulator(new BitBoardManipulatorHelper()));

        [Fact]
        public void GetBoardPositionFromNonPromotion_WhenWhitePawnE4_MovesPawnFromE2ToE4()
        {
            var prev = GetStartingPosition();
            var ply = new Ply
            {
                RawMove = "e4",
                Colour = Colour.W,
                Piece = Constants.Pieces['P'],
                IsPawnMove = true,
                IsPieceMove = true,
                IsCapture = false,
                IsPromotion = false,
                SourceSquare = 12,
                DestinationSquare = 28
            };

            var result = _sut.GetBoardPositionFromNonPromotion(prev, ply, null);

            Assert.NotNull(result);
            Assert.True((result.PiecePositions["WP"] & (1UL << 12)) == 0, "White pawn should be removed from e2");
            Assert.True((result.PiecePositions["WP"] & (1UL << 28)) != 0, "White pawn should be on e4");
        }

        [Fact]
        public void GetBoardPositionFromNonPromotion_WhenDoublePawnPush_SetsEnPassantTargetFile()
        {
            var prev = GetStartingPosition();
            var ply = new Ply
            {
                RawMove = "e4",
                Colour = Colour.W,
                Piece = Constants.Pieces['P'],
                IsPawnMove = true,
                IsPieceMove = true,
                IsCapture = false,
                SourceSquare = 12,
                DestinationSquare = 28,
                DestinationFile = 4,
                DestinationRank = 3
            };

            var result = _sut.GetBoardPositionFromNonPromotion(prev, ply, null);

            Assert.NotNull(result.EnPassantTargetFile);
            Assert.Equal('e', char.ToLowerInvariant(result.EnPassantTargetFile.Value));
        }

        [Fact]
        public void GetBoardPositionFromKingSideCastling_WhenWhite_MovesKingAndRook()
        {
            var prev = GetStartingPosition();
            var ply = new Ply { RawMove = "O-O", Colour = Colour.W, Piece = Constants.Pieces['C'], IsKingsideCastling = true };

            var result = _sut.GetBoardPositionFromKingSideCastling(prev, ply);

            Assert.True((result.PiecePositions["WK"] & (1UL << Squares.E1)) == 0);
            Assert.True((result.PiecePositions["WK"] & (1UL << Squares.G1)) != 0);
            Assert.True((result.PiecePositions["WR"] & (1UL << Squares.H1)) == 0);
            Assert.True((result.PiecePositions["WR"] & (1UL << Squares.F1)) != 0);
        }

        [Fact]
        public void GetBoardPositionFromQueenSideCastling_WhenWhite_MovesKingAndRook()
        {
            var prev = GetStartingPosition();
            var ply = new Ply { RawMove = "O-O-O", Colour = Colour.W, Piece = Constants.Pieces['C'], IsQueensideCastling = true };

            var result = _sut.GetBoardPositionFromQueenSideCastling(prev, ply);

            Assert.True((result.PiecePositions["WK"] & (1UL << Squares.E1)) == 0);
            Assert.True((result.PiecePositions["WK"] & (1UL << Squares.C1)) != 0);
            Assert.True((result.PiecePositions["WR"] & (1UL << Squares.A1)) == 0);
            Assert.True((result.PiecePositions["WR"] & (1UL << Squares.D1)) != 0);
        }

        [Fact]
        public void GetBoardPositionFromNonPromotion_WhenPieceAtDestination_Throws()
        {
            var prev = GetStartingPosition();
            var ply = new Ply
            {
                RawMove = "Nf3",
                Colour = Colour.W,
                Piece = Constants.Pieces['N'],
                IsPieceMove = true,
                IsCapture = false,
                SourceSquare = 6,
                DestinationSquare = 21
            };
            // Put a white piece on f3 so destination is occupied
            prev.PiecePositions["WP"] |= 1UL << 21;

            Assert.Throws<InvalidOperationException>(() => _sut.GetBoardPositionFromNonPromotion(prev, ply, null));
        }

        private static BoardPosition GetMinimalPosition()
        {
            var b = new BoardPosition();
            b.PiecePositions["WP"] = 0; b.PiecePositions["WN"] = 0; b.PiecePositions["WB"] = 0; b.PiecePositions["WR"] = 0; b.PiecePositions["WQ"] = 0; b.PiecePositions["WK"] = 0;
            b.PiecePositions["BP"] = 0; b.PiecePositions["BN"] = 0; b.PiecePositions["BB"] = 0; b.PiecePositions["BR"] = 0; b.PiecePositions["BQ"] = 0; b.PiecePositions["BK"] = 0;
            return b;
        }

        [Fact]
        public void GetBoardPositionFromEnPassant_WhenValidWhiteCapture_CapturesBlackPawnAndMovesWhitePawn()
        {
            // White pawn on e5 (36), Black pawn on d5 (35). White plays exd6 e.p.
            var prev = GetMinimalPosition();
            prev.PiecePositions["WP"] = 1UL << 36;
            prev.PiecePositions["BP"] = 1UL << 35;
            prev.PiecePositions["WK"] = 1UL << 4;
            prev.PiecePositions["BK"] = 1UL << 60;
            prev.EnPassantTargetFile = 'D';

            var ply = new Ply
            {
                RawMove = "exd6",
                Colour = Colour.W,
                Piece = Constants.Pieces['P'],
                IsEnpassant = true,
                IsCapture = true,
                SourceSquare = 36,
                DestinationSquare = 43,
                DestinationFile = 3
            };

            var result = _sut.GetBoardPositionFromEnPassant(prev, ply, null);

            Assert.True((result.PiecePositions["WP"] & (1UL << 36)) == 0);
            Assert.True((result.PiecePositions["WP"] & (1UL << 43)) != 0);
            Assert.True((result.PiecePositions["BP"] & (1UL << 35)) == 0);
            Assert.Null(result.EnPassantTargetFile);
        }

        [Fact]
        public void GetBoardPositionFromEnPassant_WhenNoEnPassantTarget_Throws()
        {
            var prev = GetStartingPosition();
            prev.EnPassantTargetFile = null;
            var ply = new Ply
            {
                RawMove = "exd6",
                Colour = Colour.W,
                Piece = Constants.Pieces['P'],
                IsEnpassant = true,
                DestinationFile = 3,
                SourceSquare = 36,
                DestinationSquare = 43
            };

            Assert.Throws<InvalidOperationException>(() => _sut.GetBoardPositionFromEnPassant(prev, ply, null));
        }

        [Fact]
        public void GetBoardPositionFromPromotion_WhenWhitePromotesToQueen_ReplacesPawnWithQueen()
        {
            var prev = GetMinimalPosition();
            prev.PiecePositions["WP"] = 1UL << 52; // e7
            prev.PiecePositions["WK"] = 1UL << 4;
            prev.PiecePositions["BK"] = 1UL << 63; // h8 so e8 (60) is free

            var ply = new Ply
            {
                RawMove = "e8=Q",
                Colour = Colour.W,
                Piece = Constants.Pieces['P'],
                IsPromotion = true,
                IsCapture = false,
                PromotionPiece = Constants.Pieces['Q'],
                SourceSquare = 52,
                DestinationSquare = 60
            };

            var result = _sut.GetBoardPositionFromPromotion(prev, ply, null);

            Assert.True((result.PiecePositions["WP"] & (1UL << 52)) == 0);
            Assert.True((result.PiecePositions["WQ"] & (1UL << 60)) != 0);
            Assert.Null(result.EnPassantTargetFile);
        }

        [Fact]
        public void GetBoardPositionFromPromotion_WhenSquareOccupied_Throws()
        {
            var prev = GetMinimalPosition();
            prev.PiecePositions["WP"] = 1UL << 52;
            prev.PiecePositions["BK"] = 1UL << 60; // block e8
            prev.PiecePositions["WK"] = 1UL << 4;

            var ply = new Ply
            {
                RawMove = "e8=Q",
                Colour = Colour.W,
                Piece = Constants.Pieces['P'],
                IsPromotion = true,
                IsCapture = false,
                PromotionPiece = Constants.Pieces['Q'],
                SourceSquare = 52,
                DestinationSquare = 60
            };

            Assert.Throws<InvalidOperationException>(() => _sut.GetBoardPositionFromPromotion(prev, ply, null));
        }
    }
}
