using Interfaces.DTO;
using Moq;
using Services.Helpers;
using static Interfaces.Constants;

namespace ServicesTests
{
    public class BoardPositionCalculatorTests
    {
        [Fact]
        public void GetBoardPositionFromPly_WhenEnPassant_CallsGetBoardPositionFromEnPassant()
        {
            var helperMock = new Mock<IBoardPositionCalculatorHelper>();
            var expected = new BoardPosition();
            helperMock.Setup(h => h.GetBoardPositionFromEnPassant(It.IsAny<BoardPosition>(), It.IsAny<Ply>(), It.IsAny<string>()))
                .Returns(expected);
            var sut = new BoardPositionCalculator(helperMock.Object);
            var prev = new BoardPosition();
            var ply = new Ply
            {
                RawMove = "exd6",
                Colour = Colour.W,
                Piece = Pieces['P'],
                IsEnpassant = true,
                IsPieceMove = true
            };

            var result = sut.GetBoardPositionFromPly(prev, ply, null);

            Assert.Same(expected, result);
            helperMock.Verify(h => h.GetBoardPositionFromEnPassant(prev, ply, null), Times.Once);
        }

        [Fact]
        public void GetBoardPositionFromPly_WhenPromotion_CallsGetBoardPositionFromPromotion()
        {
            var helperMock = new Mock<IBoardPositionCalculatorHelper>();
            var expected = new BoardPosition();
            helperMock.Setup(h => h.GetBoardPositionFromPromotion(It.IsAny<BoardPosition>(), It.IsAny<Ply>(), It.IsAny<string>()))
                .Returns(expected);
            var sut = new BoardPositionCalculator(helperMock.Object);
            var prev = new BoardPosition();
            var ply = new Ply
            {
                RawMove = "e8=Q",
                Colour = Colour.W,
                Piece = Pieces['P'],
                IsPieceMove = true,
                IsPromotion = true
            };

            var result = sut.GetBoardPositionFromPly(prev, ply, "ctx");

            Assert.Same(expected, result);
            helperMock.Verify(h => h.GetBoardPositionFromPromotion(prev, ply, "ctx"), Times.Once);
        }

        [Fact]
        public void GetBoardPositionFromPly_WhenNonPromotionPieceMove_CallsGetBoardPositionFromNonPromotion()
        {
            var helperMock = new Mock<IBoardPositionCalculatorHelper>();
            var expected = new BoardPosition();
            helperMock.Setup(h => h.GetBoardPositionFromNonPromotion(It.IsAny<BoardPosition>(), It.IsAny<Ply>(), It.IsAny<string>()))
                .Returns(expected);
            var sut = new BoardPositionCalculator(helperMock.Object);
            var prev = new BoardPosition();
            var ply = new Ply
            {
                RawMove = "Nf3",
                Colour = Colour.W,
                Piece = Pieces['N'],
                IsPieceMove = true,
                IsPromotion = false
            };

            var result = sut.GetBoardPositionFromPly(prev, ply, null);

            Assert.Same(expected, result);
            helperMock.Verify(h => h.GetBoardPositionFromNonPromotion(prev, ply, null), Times.Once);
        }

        [Fact]
        public void GetBoardPositionFromPly_WhenKingsideCastling_CallsGetBoardPositionFromKingSideCastling()
        {
            var helperMock = new Mock<IBoardPositionCalculatorHelper>();
            var expected = new BoardPosition();
            helperMock.Setup(h => h.GetBoardPositionFromKingSideCastling(It.IsAny<BoardPosition>(), It.IsAny<Ply>()))
                .Returns(expected);
            var sut = new BoardPositionCalculator(helperMock.Object);
            var prev = new BoardPosition();
            var ply = new Ply
            {
                RawMove = "O-O",
                Colour = Colour.W,
                Piece = Pieces['C'],
                IsKingsideCastling = true
            };

            var result = sut.GetBoardPositionFromPly(prev, ply);

            Assert.Same(expected, result);
            helperMock.Verify(h => h.GetBoardPositionFromKingSideCastling(prev, ply), Times.Once);
        }

        [Fact]
        public void GetBoardPositionFromPly_WhenQueensideCastling_CallsGetBoardPositionFromQueenSideCastling()
        {
            var helperMock = new Mock<IBoardPositionCalculatorHelper>();
            var expected = new BoardPosition();
            helperMock.Setup(h => h.GetBoardPositionFromQueenSideCastling(It.IsAny<BoardPosition>(), It.IsAny<Ply>()))
                .Returns(expected);
            var sut = new BoardPositionCalculator(helperMock.Object);
            var prev = new BoardPosition();
            var ply = new Ply
            {
                RawMove = "O-O-O",
                Colour = Colour.B,
                Piece = Pieces['C'],
                IsQueensideCastling = true
            };

            var result = sut.GetBoardPositionFromPly(prev, ply);

            Assert.Same(expected, result);
            helperMock.Verify(h => h.GetBoardPositionFromQueenSideCastling(prev, ply), Times.Once);
        }

        [Fact]
        public void GetBoardPositionFromPly_WhenUnrecognizedMoveType_Throws()
        {
            var helperMock = new Mock<IBoardPositionCalculatorHelper>();
            var sut = new BoardPositionCalculator(helperMock.Object);
            var prev = new BoardPosition();
            var ply = new Ply
            {
                RawMove = "???",
                Colour = Colour.W,
                Piece = Pieces['P'],
                IsPieceMove = false,
                IsKingsideCastling = false,
                IsQueensideCastling = false,
                IsEnpassant = false
            };

            var ex = Assert.Throws<Exception>(() => sut.GetBoardPositionFromPly(prev, ply, "PGN: g1"));
            Assert.Contains("Unrecognized move type", ex.Message);
            Assert.Contains("PGN: g1", ex.Message);
        }
    }
}
