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
    }
}
