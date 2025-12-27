using Interfaces.DTO;
using Moq;
using Services;
using Services.Helpers.BoardUpdater;
using static Interfaces.Constants;

namespace ServicesTests.Helpers;

public class UpdaterQueensideCastlingTests
{
    private readonly Mock<IBitBoardManipulator> _mockBitBoardManipulator;
    private readonly UpdaterQueensideCastling _updater;

    public UpdaterQueensideCastlingTests()
    {
        _mockBitBoardManipulator = new Mock<IBitBoardManipulator>();
        _updater = new UpdaterQueensideCastling(_mockBitBoardManipulator.Object);

        _mockBitBoardManipulator
            .Setup(m => m.MovePiece(It.IsAny<ulong>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns((ulong current, int source, int dest) => (ulong)(1UL << dest));
    }

    [Fact]
    public void UpdateBoard_WhiteQueensideCastling_MovesKingAndRookCorrectly()
    {
        // Arrange
        var currentBoardPosition = new BoardPosition();

        // Initial positions: E1 (4) and A1 (0)
        ulong initialWhiteKingBoard = 1UL << 4;
        ulong initialWhiteRookBoard = 1UL << 0;

        currentBoardPosition.PiecePositions["WK"] = initialWhiteKingBoard;
        currentBoardPosition.PiecePositions["WR"] = initialWhiteRookBoard;

        var ply = new Ply { Colour = Colour.W, RawMove = "" };

        // Expected end positions: C1 (2) and D1 (3)
        ulong expectedKingEndPosition = 1UL << 2;
        ulong expectedRookEndPosition = 1UL << 3;

        // Act
        // Source/Dest parameters of the main method are effectively ignored by this specific implementation
        _updater.UpdateBoard(currentBoardPosition, "someKey", ply, 4, 2);

        // Assert
        Assert.Equal(expectedKingEndPosition, currentBoardPosition.PiecePositions["WK"]);
        Assert.Equal(expectedRookEndPosition, currentBoardPosition.PiecePositions["WR"]);

        // Verify exact calls to the manipulator for the King (E1 to C1)
        _mockBitBoardManipulator.Verify(
            m => m.MovePiece(initialWhiteKingBoard, 4, 2),
            Times.Once(),
            "White King move parameters were incorrect for queenside castling.");

        // Verify exact calls to the manipulator for the Rook (A1 to D1)
        _mockBitBoardManipulator.Verify(
            m => m.MovePiece(initialWhiteRookBoard, 0, 3),
            Times.Once(),
            "White Rook move parameters were incorrect for queenside castling.");
    }

    [Fact]
    public void UpdateBoard_BlackQueensideCastling_MovesKingAndRookCorrectly()
    {
        // Arrange
        var currentBoardPosition = new BoardPosition();

        // Initial positions: E8 (60) and A8 (56)
        ulong initialBlackKingBoard = 1UL << 60;
        ulong initialBlackRookBoard = 1UL << 56;

        currentBoardPosition.PiecePositions["BK"] = initialBlackKingBoard;
        currentBoardPosition.PiecePositions["BR"] = initialBlackRookBoard;

        var ply = new Ply { Colour = Colour.B, RawMove = "" };

        // Expected end positions: C8 (58) and D8 (59)
        ulong expectedKingEndPosition = 1UL << 58;
        ulong expectedRookEndPosition = 1UL << 59;

        // Act
        _updater.UpdateBoard(currentBoardPosition, "someKey", ply, 60, 58);

        // Assert
        Assert.Equal(expectedKingEndPosition, currentBoardPosition.PiecePositions["BK"]);
        Assert.Equal(expectedRookEndPosition, currentBoardPosition.PiecePositions["BR"]);

        // Verify exact calls to the manipulator for the King (E8 to C8)
        _mockBitBoardManipulator.Verify(
            m => m.MovePiece(initialBlackKingBoard, 60, 58),
            Times.Once(),
            "Black King move parameters were incorrect for queenside castling.");

        // Verify exact calls to the manipulator for the Rook (A8 to D8)
        _mockBitBoardManipulator.Verify(
            m => m.MovePiece(initialBlackRookBoard, 56, 59),
            Times.Once(),
            "Black Rook move parameters were incorrect for queenside castling.");
    }
}
