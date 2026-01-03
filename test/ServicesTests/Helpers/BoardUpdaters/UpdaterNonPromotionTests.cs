using Interfaces;
using Interfaces.DTO;
using Moq;
using Services;
using Services.Helpers;
using Services.Helpers.BoardUpdater;
using static Interfaces.Constants;

namespace ServicesTests.Helpers.BoardUpdaters
{
    public class UpdaterNonPromotionTests
    {
        private readonly Mock<IBitBoardManipulator> _mockManipulator;
        private readonly UpdaterNonPromotion _updater;

        public UpdaterNonPromotionTests()
        {
            _mockManipulator = new Mock<IBitBoardManipulator>();
            // Initialize the class under test, injecting the mock
            _updater = new UpdaterNonPromotion(_mockManipulator.Object);
        }

        [Fact(DisplayName = "A standard move (non-capture) should update the source piece position only")]
        public void UpdateBoard_NonCapture_UpdatesOnlyMovingPiecePosition()
        {
            // Arrange
            var board = SetupStandardBoard();
            const string movingPieceKey = "WP"; // White Pawn
            board.PiecePositions[movingPieceKey] = 0b1ul; // Piece at Square 0

            var ply = new Ply { IsCapture = false, Colour = Colour.W, RawMove = "" };
            const int source = 0, dest = 8;
            const ulong expectedNewPosition = 0b100000001ul; // Move from 0 to 8

            // Mock the manipulator's behavior: when asked to move the piece, return the expected new position
            _mockManipulator
                .Setup(m => m.MovePiece(It.IsAny<ulong>(), source, dest))
                .Returns(expectedNewPosition);

            // Act
            _updater.UpdateBoard(board, movingPieceKey, ply, source, dest);

            // Assert
            // Verify the board state has been updated correctly for the moving piece
            Assert.Equal(expectedNewPosition, board.PiecePositions[movingPieceKey]);

            // Verify that the other pieces on the board were NOT modified (no interaction with RemovePiece)
            _mockManipulator.Verify(m => m.RemovePiece(It.IsAny<ulong>(), It.IsAny<int>()), Times.Never);

            // Verify that the correct internal method was called exactly once
            _mockManipulator.Verify(m => m.MovePiece(0b1ul, source, dest), Times.Once);
        }

        [Fact(DisplayName = "A capture move should update the moving piece AND remove the opposing piece at destination")]
        public void UpdateBoard_Capture_RemovesOpposingPieceAndMovesOwnPiece()
        {
            // Arrange
            var board = SetupStandardBoard();
            const string movingPieceKey = "BP"; // Black Pawn
            const string capturedPieceKey = "WP"; // White Pawn

            const int source = 50, dest = 42; // Example squares
            const ulong initialBlackPawnPos = (1ul << source);
            const ulong initialWhitePawnPos = (1ul << dest);
            const ulong expectedNewBlackPawnPos = (1ul << dest); // Black pawn moves to capture square
            const ulong expectedEmptyWhitePawnPos = 0ul; // White pawn is removed

            board.PiecePositions[movingPieceKey] = initialBlackPawnPos;
            board.PiecePositions[capturedPieceKey] = initialWhitePawnPos;

            var ply = new Ply { IsCapture = true, Colour = Colour.B, RawMove = "" }; // Black is capturing White

            // Mock the manipulator's behavior for the moving piece
            _mockManipulator
                .Setup(m => m.MovePiece(initialBlackPawnPos, source, dest))
                .Returns(expectedNewBlackPawnPos);

            // Mock the manipulator's behavior for the captured piece (RemovePiece should result in 0)
            _mockManipulator
                .Setup(m => m.RemovePiece(initialWhitePawnPos, dest))
                .Returns(expectedEmptyWhitePawnPos);

            // Act
            _updater.UpdateBoard(board, movingPieceKey, ply, source, dest);

            // Assert
            // Verify the moving piece's position is updated correctly
            Assert.Equal(expectedNewBlackPawnPos, board.PiecePositions[movingPieceKey]);

            // Verify the captured piece's position is now empty (or whatever RemovePiece returned)
            Assert.Equal(expectedEmptyWhitePawnPos, board.PiecePositions[capturedPieceKey]);

            // Verify interactions with Mocks
            // RemovePiece should have been called for all potential white pieces, but only the WP entry had data
            _mockManipulator.Verify(m => m.RemovePiece(It.IsAny<ulong>(), dest), Times.AtLeastOnce);
            _mockManipulator.Verify(m => m.MovePiece(initialBlackPawnPos, source, dest), Times.Once);
        }

        [Fact(DisplayName = "A capture move iterates through all opponent piece types to ensure removal")]
        public void UpdateBoard_Capture_IteratesThroughAllOpponentPieceKeys()
        {
            // Arrange
            var board = SetupStandardBoard();
            const string movingPieceKey = "WP";
            const int source = 0, dest = 8;
            var ply = new Ply { IsCapture = true, Colour = Colour.W, RawMove = "" };

            // Act
            _updater.UpdateBoard(board, movingPieceKey, ply, source, dest);

            // Assert
            // We ensure that RemovePiece was called for every single Black piece type defined in Constants.PieceIndex.Keys
            // The exact count depends on your Constants definition (e.g., 6 pieces: P, N, B, R, Q, K)
            int expectedRemoveCalls = Constants.PieceIndex.Keys.Count;

            // Moq can verify how many times a method was called with a specific destination square
            _mockManipulator.Verify(m => m.RemovePiece(It.IsAny<ulong>(), dest), Times.Exactly(expectedRemoveCalls));
        }


        

        // --- Edge Case 1: Piece Not Found at Source ---

        [Fact(DisplayName = "If the source square is unexpectedly empty, the move result is still applied")]
        public void UpdateBoard_SourceSquareIsEmptyInBitboard_AppliesMoveResultAnyway()
        {
            // Arrange
            var board = SetupStandardBoard();
            const string movingPieceKey = "WP";
            // Note: board.PiecePositions["WP"] is 0ul (empty)
            const int source = 10, dest = 18;
            var ply = new Ply { IsCapture = false, Colour = Colour.W , RawMove = "" };

            // Mock the return value of MovePiece assuming it handles the logic
            // We assume the manipulator correctly calculates the new position should be just a piece at 'dest'
            _mockManipulator.Setup(m => m.MovePiece(0ul, source, dest))
                            .Returns(1ul << dest);

            // Act
            _updater.UpdateBoard(board, movingPieceKey, ply, source, dest);

            // Assert
            // The resulting board position for "WP" should now only contain the destination piece
            Assert.Equal(1ul << dest, board.PiecePositions[movingPieceKey]);

            // Verify that the helper method was called with the empty source board (0ul)
            _mockManipulator.Verify(m => m.MovePiece(0ul, source, dest), Times.Once);
        }

        // --- Edge Case 2: Destination Already Occupied (Non-Capture) ---
        [Fact(DisplayName = "A non-capture move to an occupied square throws an exception")]
        public void UpdateBoard_NonCaptureMoveToOccupiedSquare_ShouldThrowException()
        {
            // Arrange
            var mockBitBoardManipulatorHelper = new Mock<IBitBoardManipulatorHelper>();
            var bitBoardManipulator = new BitBoardManipulator(mockBitBoardManipulatorHelper.Object);
            var updatorNonPromotion = new UpdaterNonPromotion(bitBoardManipulator);

            var board = SetupStandardBoard();
            const string movingPieceKey = "WP"; // White Pawn
            const string occupyingPieceKey = "BN"; // White Knight
            const int source = 0;
            const int dest = 8; 
            board.PiecePositions[movingPieceKey] = (1ul << source); // Piece at Square A1 (source)
            board.PiecePositions[occupyingPieceKey] = (1ul << dest); // Piece at Square A2 (destination)
            var ply = new Ply { IsCapture = false, Colour = Colour.W, RawMove = "" };
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                updatorNonPromotion.UpdateBoard(board, movingPieceKey, ply, source, dest)
            );
            Assert.Equal($"There is an opposing piece at the destination square {dest} for a non-capture move.", exception.Message);
        }

        // --- Edge Case 3: Capture Flagged, But No Piece to Capture ---

        [Fact(DisplayName = "A capture move where the destination is empty executes without error")]
        public void UpdateBoard_CaptureFlaggedButNoOpponentPieceExists_ExecutesWithoutError()
        {
            // Arrange
            var mockBitBoardManipulatorHelper = new Mock<IBitBoardManipulatorHelper>();
            var bitBoardManipulator = new BitBoardManipulator(mockBitBoardManipulatorHelper.Object);
            var updatorNonPromotion = new UpdaterNonPromotion(bitBoardManipulator);

            var board = SetupStandardBoard();
            const string movingPieceKey = "WP";

            // Move a pawn from A2 to A3
            const int source = 8; // corresponds to square A2
            const int dest = 16; // corresponds to square A3

            // Ensure that the moving piece is at the source square. This could be set up with just 1ul,
            // but using the shift makes it clear which square is occupied.
            // (the bitwise << operator shifts the 1 bit to the left by 'source' positions - here shifted by zero)
            board.PiecePositions[movingPieceKey] = (1ul << source); // Ensure the moving piece is present
            // All black pieces are initialized as 0ul (empty)

            var ply = new Ply { IsCapture = true, Colour = Colour.W, RawMove = "" }; // Flagged as capture

            // Act
            updatorNonPromotion.UpdateBoard(board, movingPieceKey, ply, source, dest);

            // Assert
            // The white piece should have moved correctly
            // Since there was no opposing piece at 'dest', no removal occurs, but the move still happens
            Assert.Equal((1ul << dest), board.PiecePositions[movingPieceKey]);            
        }

        [Fact(DisplayName = "A capture move where the destination is empty executes without error (using Mock)")]
        public void UpdateBoard_CaptureFlaggedButNoOpponentPieceExists_CallsRemovePieceCorrectly()
        {
            var bitBoardManipulatorMock = new Mock<IBitBoardManipulator>();

            var updaterNonPromotion = new UpdaterNonPromotion(bitBoardManipulatorMock.Object);

            var board = SetupStandardBoard();
            const string movingPieceKey = "WP";

            // Move a pawn from A2 to A3
            const int source = 8; // corresponds to square A2
            const int dest = 16; // corresponds to square A3

            // Ensure that the moving piece is at the source square. This could be set up with just 1ul,
            // but using the shift makes it clear which square is occupied.
            // (the bitwise << operator shifts the 1 bit to the left by 'source' positions - here shifted by zero)
            board.PiecePositions[movingPieceKey] = (1ul << source); // Ensure the moving piece is present
            // All black pieces are initialized as 0ul (empty)

            var ply = new Ply { IsCapture = true, Colour = Colour.W, RawMove = "" }; // Flagged as capture

            // Act
            updaterNonPromotion.UpdateBoard(board, movingPieceKey, ply, source, dest);

            // Assert
            // The RemovePiece operation was called many times for empty boards, which is fine
            int expectedRemoveCalls = Constants.PieceIndex.Keys.Count; // e.g., 6 calls for P, N, B, R, Q, K

            // TODO. check that the bitBoardManipulator's RemovePiece has been called 6 times

            // Verify only works on mocks
            bitBoardManipulatorMock.Verify(m => m.RemovePiece(0ul, dest), Times.Exactly(expectedRemoveCalls));
        }

        [Fact(DisplayName = "Using a piece key not in the dictionary throws a KeyNotFoundException")]
        public void UpdateBoard_WithInvalidPiecePositionsKey_ThrowsKeyNotFoundException()
        {
            // Arrange
            var board = SetupStandardBoard();
            const string invalidKey = "WQ_INVALID"; // Does not exist in the dictionary
            const int source = 0, dest = 8;
            var ply = new Ply { IsCapture = false, Colour = Colour.W, RawMove = "" };

            // Act & Assert
            // We expect the standard System.Collections.Generic.KeyNotFoundException from the dictionary access
            Assert.Throws<KeyNotFoundException>(() =>
                _updater.UpdateBoard(board, invalidKey, ply, source, dest)
            );
        }

        

        private BoardPosition SetupStandardBoard()
        {
            var bp = new BoardPosition();
            // Initialize a standard set of empty ulongs for all piece types for both colors
            foreach (var color in new[] { 'W', 'B' })
            {
                foreach (var piece in Constants.PieceIndex.Keys)
                {
                    bp.PiecePositions[new([color, piece])] = 0ul;
                }
            }
            return bp;
        }
    }
}
