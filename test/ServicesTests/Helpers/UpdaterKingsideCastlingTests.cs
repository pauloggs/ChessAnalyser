using Interfaces.DTO;
using Moq;
using Services;
using Services.Helpers.BoardUpdater;
using static Interfaces.Constants;

namespace ServicesTests.Helpers
{
    public class UpdaterKingsideCastlingTests
    {
        private readonly Mock<IBitBoardManipulator> _mockBitBoardManipulator;
        private readonly UpdaterKingsideCastling _updater;

        public UpdaterKingsideCastlingTests()
        {
            // Initialize the mock and the system under test (SUT)
            _mockBitBoardManipulator = new Mock<IBitBoardManipulator>();
            _updater = new UpdaterKingsideCastling(_mockBitBoardManipulator.Object);

            // Setup common mock behavior for PiecePositionsAfterMove
            // We configure the mock to simply return a bitboard representing the destination square
            // for verification purposes.
            _mockBitBoardManipulator
                .Setup(m => m.PiecePositionsAfterMove(It.IsAny<ulong>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((ulong current, int source, int dest) => (ulong)(1UL << dest));
        }

        [Fact]
        public void UpdateBoard_WhiteKingsideCastling_MovesKingAndRookCorrectly()
        {
            // Arrange
            // Using your specific BoardPosition DTO structure for initialization
            var currentBoardPosition = new BoardPosition();

            // Set initial positions: E1 (4) and H1 (7)
            ulong initialWhiteKingBoard = 1UL << 4;
            ulong initialWhiteRookBoard = 1UL << 7;

            currentBoardPosition.PiecePositions["WK"] = initialWhiteKingBoard;
            currentBoardPosition.PiecePositions["WR"] = initialWhiteRookBoard;

            // Note: The source/destination squares of the *Ply* object itself are ignored by this specific updater, 
            // as it uses hardcoded castling squares. We need to define a simple Ply class/interface to compile:
            var ply = new Ply { Colour = Colour.W, RawMove = "" };

            // Expected end positions: G1 (6) and F1 (5)
            ulong expectedKingEndPosition = 1UL << 6;
            ulong expectedRookEndPosition = 1UL << 5;

            // Act
            // sourceSquare and destinationSquare parameters passed to UpdateBoard are ignored internally
            _updater.UpdateBoard(currentBoardPosition, "someKey", ply, 4, 6);

            // Assert
            // Verify the board position DTO was updated with the new values
            Assert.Equal(expectedKingEndPosition, currentBoardPosition.PiecePositions["WK"]);
            Assert.Equal(expectedRookEndPosition, currentBoardPosition.PiecePositions["WR"]);

            // Verify that the underlying IBitBoardManipulator was called exactly twice with the correct parameters
            _mockBitBoardManipulator.Verify(
                m => m.PiecePositionsAfterMove(initialWhiteKingBoard, 4, 6),
                Times.Once(),
                "King move parameters were incorrect.");

            _mockBitBoardManipulator.Verify(
                m => m.PiecePositionsAfterMove(initialWhiteRookBoard, 7, 5),
                Times.Once(),
                "Rook move parameters were incorrect.");
        }

        [Fact]
        public void UpdateBoard_BlackKingsideCastling_MovesKingAndRookCorrectly()
        {
            // Arrange
            var currentBoardPosition = new BoardPosition();

            // Set initial positions: E8 (60) and H8 (63)
            ulong initialBlackKingBoard = 1UL << 60;
            ulong initialBlackRookBoard = 1UL << 63;

            currentBoardPosition.PiecePositions["BK"] = initialBlackKingBoard;
            currentBoardPosition.PiecePositions["BR"] = initialBlackRookBoard;

            var ply = new Ply { Colour = Colour.B, RawMove = "" };

            // Expected end positions: G8 (62) and F8 (61)
            ulong expectedKingEndPosition = 1UL << 62;
            ulong expectedRookEndPosition = 1UL << 61;

            // Act
            // sourceSquare and destinationSquare parameters passed to UpdateBoard are ignored internally
            _updater.UpdateBoard(currentBoardPosition, "someKey", ply, 60, 62);

            // Assert
            // Verify the board position DTO was updated with the new values
            Assert.Equal(expectedKingEndPosition, currentBoardPosition.PiecePositions["BK"]);
            Assert.Equal(expectedRookEndPosition, currentBoardPosition.PiecePositions["BR"]);

            // Verify that the underlying IBitBoardManipulator was called exactly twice with the correct parameters
            _mockBitBoardManipulator.Verify(
                m => m.PiecePositionsAfterMove(initialBlackKingBoard, 60, 62),
                Times.Once(),
                "Black King move parameters were incorrect.");

            _mockBitBoardManipulator.Verify(
                m => m.PiecePositionsAfterMove(initialBlackRookBoard, 63, 61),
                Times.Once(),
                "Black Rook move parameters were incorrect.");
        }
    }
}
