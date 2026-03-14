using Interfaces.DTO;
using Moq;
using Services;
using Services.Helpers;
using static Interfaces.Constants;

namespace ServicesTests
{
    /// <summary>
    /// Tests for MoveInterpreter using the real MoveInterpreterHelper and full dependency chain
    /// (DestinationSquareHelper, PawnMoveInterpreter, PieceMoveInterpreter, PieceSourceFinderService, etc.).
    /// These verify the pipeline from raw move + board to (piece, sourceSquare, destinationSquare).
    /// </summary>
    public class MoveInterpreterWithRealHelperTests
    {
        private static IMoveInterpreter CreateMoveInterpreterWithRealChain()
        {
            var bitBoardHelper = new BitBoardManipulatorHelper();
            var bitBoardManipulator = new BitBoardManipulator(bitBoardHelper);
            var sourceSquareHelper = new SourceSquareHelper(bitBoardManipulator);
            var rankAndFileHelper = new RankAndFileHelper();
            var legalMoveChecker = new LegalMoveChecker(bitBoardManipulator);
            var pieceSourceFinderService = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper, bitBoardManipulator, legalMoveChecker);
            var pawnMoveInterpreter = new PawnMoveInterpreter(bitBoardManipulator);
            var pieceMoveInterpreter = new PieceMoveInterpreter(sourceSquareHelper, pieceSourceFinderService);
            var destinationSquareHelper = new DestinationSquareHelper();
            var moveInterpreterHelper = new MoveInterpreterHelper(destinationSquareHelper, pawnMoveInterpreter, pieceMoveInterpreter);
            return new MoveInterpreter(moveInterpreterHelper);
        }

        private static BoardPosition GetStartingPosition()
        {
            var displayMock = new Mock<IDisplayService>();
            var calculatorMock = new Mock<IBoardPositionCalculator>();
            var moveInterpreter = CreateMoveInterpreterWithRealChain();
            var positionsHelper = new BoardPositionsHelper(moveInterpreter, displayMock.Object, calculatorMock.Object);
            return positionsHelper.GetStartingBoardPosition();
        }

        [Fact]
        public void GetSourceAndDestinationSquares_WhenE4FromStartingPosition_ReturnsPawnE2ToE4()
        {
            var sut = CreateMoveInterpreterWithRealChain();
            var board = GetStartingPosition();
            var ply = new Ply { RawMove = "e4", Colour = Colour.W };

            var (piece, sourceSquare, destinationSquare) = sut.GetSourceAndDestinationSquares(board, ply);

            Assert.Equal(Pieces['P'], piece);
            Assert.True(ply.IsPawnMove);
            Assert.Equal(12, sourceSquare);   // e2
            Assert.Equal(28, destinationSquare); // e4
        }

        [Fact]
        public void GetSourceAndDestinationSquares_WhenNf3FromStartingPosition_ReturnsKnightG1ToF3()
        {
            var sut = CreateMoveInterpreterWithRealChain();
            var board = GetStartingPosition();
            var ply = new Ply { RawMove = "Nf3", Colour = Colour.W };

            var (piece, sourceSquare, destinationSquare) = sut.GetSourceAndDestinationSquares(board, ply);

            Assert.Equal(Pieces['N'], piece);
            Assert.False(ply.IsPawnMove);
            Assert.Equal(Squares.G1, sourceSquare);   // 6
            Assert.Equal(21, destinationSquare);     // f3
        }

        [Fact]
        public void GetSourceAndDestinationSquares_WhenE4Check_StripsCheckAndReturnsCorrectSquares()
        {
            var sut = CreateMoveInterpreterWithRealChain();
            var board = GetStartingPosition();
            var ply = new Ply { RawMove = "e4+", Colour = Colour.W };

            var (piece, sourceSquare, destinationSquare) = sut.GetSourceAndDestinationSquares(board, ply);

            Assert.Equal("e4", ply.RawMove);
            Assert.True(ply.IsCheck);
            Assert.Equal(12, sourceSquare);
            Assert.Equal(28, destinationSquare);
        }
    }
}
