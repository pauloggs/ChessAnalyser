using Interfaces.DTO;
using Services;

namespace ServicesTests
{
    public class DisplayServiceTests
    {
        private readonly IDisplayService _sut = new DisplayService();

        [Fact]
        public void GetBoardArrayFromBoardPositions_WhenSinglePiece_ReturnsCorrectArray()
        {
            var board = new BoardPosition();
            board.PiecePositions["WP"] = 1UL << 0; // a1

            var result = _sut.GetBoardArrayFromBoardPositions(board);

            Assert.NotNull(result);
            Assert.Equal(8, result.GetLength(0));
            Assert.Equal(8, result.GetLength(1));
            // DisplayService: LSB of byte is files[7], so bit 0 (a1) => result[0, 7]
            Assert.Equal((sbyte)1, result[0, 7]);
        }

        [Fact]
        public void GetBoardArrayFromBoardPositions_WhenBlackPiece_ReturnsPieceTypeValue()
        {
            var board = new BoardPosition();
            // b8 = square 57; rank 7 => ranks[7], file 1 => bit 1 of that byte => files[6]='1' => fileIndex 6
            board.PiecePositions["BN"] = 1UL << 57;

            var result = _sut.GetBoardArrayFromBoardPositions(board);

            Assert.Equal((sbyte)2, result[7, 6]);
        }

        [Fact]
        public void GetBoardArrayFromBoardPositions_WhenEmptyBoard_ReturnsZeros()
        {
            var board = new BoardPosition();

            var result = _sut.GetBoardArrayFromBoardPositions(board);

            for (var r = 0; r < 8; r++)
            for (var f = 0; f < 8; f++)
                Assert.Equal((sbyte)0, result[r, f]);
        }
    }
}
