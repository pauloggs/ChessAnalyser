using Interfaces.DTO;
using Services.Helpers;
using static Interfaces.Constants;

namespace ServicesHelpersTests
{
    public class DestinationSquareHelperTests
    {
        private readonly IDestinationSquareHelper _sut = new DestinationSquareHelper();

        [Fact]
        public void GetDestinationSquare_WhenPieceMoveNf3_ReturnsSquare21AndSetsDestinationRankAndFile()
        {
            var ply = new Ply
            {
                RawMove = "Nf3",
                IsPieceMove = true,
                IsPromotion = false
            };

            var result = _sut.GetDestinationSquare(ply);

            Assert.Equal(21, result);
            Assert.Equal(2, ply.DestinationRank);
            Assert.Equal(5, ply.DestinationFile);
        }

        [Fact]
        public void GetDestinationSquare_WhenPieceMoveE4_ReturnsSquare28()
        {
            var ply = new Ply
            {
                RawMove = "e4",
                IsPieceMove = true,
                IsPawnMove = true,
                IsPromotion = false
            };

            var result = _sut.GetDestinationSquare(ply);

            Assert.Equal(28, result);
            Assert.Equal(3, ply.DestinationRank);
            Assert.Equal(4, ply.DestinationFile);
        }

        [Fact]
        public void GetDestinationSquare_WhenPromotionE8Q_ReturnsSquare60()
        {
            var ply = new Ply
            {
                RawMove = "e8=Q",
                IsPieceMove = true,
                IsPromotion = true
            };

            var result = _sut.GetDestinationSquare(ply);

            Assert.Equal(60, result);
            Assert.Equal(7, ply.DestinationRank);
            Assert.Equal(4, ply.DestinationFile);
        }

        [Fact]
        public void GetDestinationSquare_WhenKingsideCastling_ReturnsMinusOne()
        {
            var ply = new Ply
            {
                RawMove = "O-O",
                IsKingsideCastling = true,
                IsPieceMove = false
            };

            var result = _sut.GetDestinationSquare(ply);

            Assert.Equal(-1, result);
        }

        [Fact]
        public void GetDestinationSquare_WhenQueensideCastling_ReturnsMinusOne()
        {
            var ply = new Ply
            {
                RawMove = "O-O-O",
                IsQueensideCastling = true,
                IsPieceMove = false
            };

            var result = _sut.GetDestinationSquare(ply);

            Assert.Equal(-1, result);
        }

        [Fact]
        public void GetDestinationSquare_WhenCaptureBxe5_ReturnsSquare36()
        {
            var ply = new Ply
            {
                RawMove = "Bxe5",
                IsPieceMove = true,
                IsPromotion = false
            };

            var result = _sut.GetDestinationSquare(ply);

            Assert.Equal(36, result);
            Assert.Equal(4, ply.DestinationRank);
            Assert.Equal(4, ply.DestinationFile);
        }
    }
}
