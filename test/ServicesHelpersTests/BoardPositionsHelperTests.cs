using Interfaces.DTO;
using Moq;
using Services;
using Services.Helpers;
using static Interfaces.Constants;

namespace ServicesHelpersTests
{
    public class BoardPositionsHelperTests
    {
        private readonly Mock<IMoveInterpreter> moveInterpreterMock;
        private readonly Mock<IDisplayService> displayServiceMock;
        private readonly Mock<IBoardPositionCalculator> boardPositionCalculatorMock;
        private readonly IBoardPositionsHelper sut;

        public BoardPositionsHelperTests()
        {
            moveInterpreterMock = new Mock<IMoveInterpreter>();
            displayServiceMock = new Mock<IDisplayService>();
            boardPositionCalculatorMock = new Mock<IBoardPositionCalculator>();
            sut = new BoardPositionsHelper(
                moveInterpreterMock.Object,
                displayServiceMock.Object,
                boardPositionCalculatorMock.Object);
        }

        [Fact]
        public void GetStartingBoardPosition_ReturnsValidStartingPosition()
        {
            // Act
            var startingPosition = sut.GetStartingBoardPosition();

            // Assert
            Assert.NotNull(startingPosition);
            Assert.NotNull(startingPosition.PiecePositions);
        }

        [Fact]
        public void GetStartingBoardPosition_ReturnsStandardChessLayout_AllTwelvePieceBitboardsCorrect()
        {
            var expected = new Dictionary<string, ulong>
            {
                ["WP"] = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_0000_0000,
                ["WN"] = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0100_0010,
                ["WB"] = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0010_0100,
                ["WR"] = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1000_0001,
                ["WQ"] = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1000,
                ["WK"] = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0001_0000,
                ["BP"] = 0b_0000_0000_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000,
                ["BN"] = 0b_0100_0010_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000,
                ["BB"] = 0b_0010_0100_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000,
                ["BR"] = 0b_1000_0001_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000,
                ["BQ"] = 0b_0000_1000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000,
                ["BK"] = 0b_0001_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000
            };

            var startingPosition = sut.GetStartingBoardPosition();

            Assert.NotNull(startingPosition.PiecePositions);
            Assert.Equal(12, startingPosition.PiecePositions.Count);
            foreach (var kv in expected)
            {
                Assert.True(startingPosition.PiecePositions.ContainsKey(kv.Key), $"Missing piece key: {kv.Key}");
                Assert.Equal(kv.Value, startingPosition.PiecePositions[kv.Key]);
            }
            Assert.Null(startingPosition.EnPassantTargetFile);
        }

        [Fact]
        public void SetWinner_Player1Wins_ReturnsTrueAndSetsWinner()
        {
            // Arrange
            var game = new Game
            {
                Name = "Test Game",
                Plies = new Dictionary<int, Ply>
                {
                    { 0, new Ply { RawMove = "1-0" } }
                }
            };

            // Act
            var result = sut.SetWinner(game, 0);

            // Assert
            Assert.True(result);
            Assert.Equal("W", game.Winner);
        }

        [Fact]
        public void SetWinner_Player2Wins_ReturnsTrueAndSetsWinner()
        {
            // Arrange
            var game = new Game
            {
                Name = "Test Game",
                Plies = new Dictionary<int, Ply>
                {
                    { 0, new Ply { RawMove = "0-1" } }
                }
            };

            // Act
            var result = sut.SetWinner(game, 0);

            // Assert
            Assert.True(result);
            Assert.Equal("B", game.Winner);
        }

        [Fact]
        public void SetWinner_DrawCondition_ReturnsTrueAndSetsWinner()
        {
            // Arrange
            var game = new Game
            {
                Name = "Test Game",
                Plies = new Dictionary<int, Ply>
                {
                    { 0, new Ply { RawMove = "1/2-1/2" } }
                }
            };

            // Act
            var result = sut.SetWinner(game, 0);

            // Assert
            Assert.True(result);
            Assert.Equal("D", game.Winner);
        }

        [Fact]
        public void SetWinner_InvalidMove_ReturnsFalse()
        {
            // Arrange
            var game = new Game
            {
                Name = "Test Game",
                Plies = new Dictionary<int, Ply>
                {
                    { 0, new Ply { RawMove = "invalid" } }
                }
            };

            // Act
            var result = sut.SetWinner(game, 0);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetWinner_EmptyMove_ReturnsFalse()
        {
            // Arrange
            var game = new Game
            {
                Name = "Test Game",
                Plies = new Dictionary<int, Ply>
                {
                    { 0, new Ply { RawMove = "" } }
                }
            };

            // Act
            var result = sut.SetWinner(game, 0);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetWinner_NullGame_ThrowsArgumentNullException()
        {
            // Arrange
            Game game = null;
            int plyIndex = 0;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.SetWinner(game, plyIndex));
        }

        [Fact]
        public void SetWinner_NoPlies_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var game = new Game
            {
                Name = "Test Game",
                Plies = []
            };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.SetWinner(game, 0));
        }

        [Fact]
        public void GetBoardPositionForPly_WhenPlyIndexZero_UsesInitialBoardPositionAndCallsMoveInterpreterAndCalculator()
        {
            var initialPosition = sut.GetStartingBoardPosition();
            var game = new Game
            {
                Name = "G",
                SourcePgnFileName = "file.pgn",
                GameIndexInFile = 1,
                InitialBoardPosition = initialPosition,
                Plies = new Dictionary<int, Ply> { { 0, new Ply { RawMove = "e4", Colour = Colour.W } } },
                BoardPositions = new Dictionary<int, BoardPosition>()
            };
            var expectedNewPosition = new BoardPosition();
            moveInterpreterMock
                .Setup(m => m.GetSourceAndDestinationSquares(initialPosition, game.Plies[0]))
                .Returns((Pieces['P'], 12, 28));
            boardPositionCalculatorMock
                .Setup(c => c.GetBoardPositionFromPly(initialPosition, It.IsAny<Ply>(), It.IsAny<string>()))
                .Returns(expectedNewPosition);

            var result = sut.GetBoardPositionForPly(game, 0);

            Assert.Same(expectedNewPosition, result);
            Assert.Equal(Pieces['P'], game.Plies[0].Piece);
            Assert.Equal(12, game.Plies[0].SourceSquare);
            Assert.Equal(28, game.Plies[0].DestinationSquare);
            boardPositionCalculatorMock.Verify(c => c.GetBoardPositionFromPly(
                initialPosition,
                It.Is<Ply>(p => p.SourceSquare == 12 && p.DestinationSquare == 28),
                It.Is<string>(s => s != null && s.Contains("file.pgn") && s.Contains("Game #1") && s.Contains("e4"))),
                Times.Once);
        }

        [Fact]
        public void GetBoardPositionForPly_WhenPlyIndexOne_UsesPreviousBoardPosition()
        {
            var initialPosition = sut.GetStartingBoardPosition();
            var afterPly0 = new BoardPosition();
            var game = new Game
            {
                Name = "G",
                InitialBoardPosition = initialPosition,
                Plies = new Dictionary<int, Ply>
                {
                    { 0, new Ply { RawMove = "e4", Colour = Colour.W } },
                    { 1, new Ply { RawMove = "e5", Colour = Colour.B } }
                },
                BoardPositions = new Dictionary<int, BoardPosition> { { 0, afterPly0 } }
            };
            var expectedNewPosition = new BoardPosition();
            moveInterpreterMock
                .Setup(m => m.GetSourceAndDestinationSquares(afterPly0, game.Plies[1]))
                .Returns((Pieces['P'], 52, 36));
            boardPositionCalculatorMock
                .Setup(c => c.GetBoardPositionFromPly(afterPly0, It.IsAny<Ply>(), It.IsAny<string>()))
                .Returns(expectedNewPosition);

            var result = sut.GetBoardPositionForPly(game, 1);

            Assert.Same(expectedNewPosition, result);
            moveInterpreterMock.Verify(m => m.GetSourceAndDestinationSquares(afterPly0, game.Plies[1]), Times.Once);
        }

        [Fact]
        public void GetBoardPositionForPly_WhenGameIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => sut.GetBoardPositionForPly(null!, 0));
        }

        [Fact]
        public void GetBoardPositionForPly_WhenPlyZeroAndInitialBoardPositionNull_ThrowsInvalidOperationException()
        {
            var game = new Game
            {
                Name = "G",
                InitialBoardPosition = null,
                Plies = new Dictionary<int, Ply> { { 0, new Ply { RawMove = "e4", Colour = Colour.W } } },
                BoardPositions = new Dictionary<int, BoardPosition>()
            };

            var ex = Assert.Throws<InvalidOperationException>(() => sut.GetBoardPositionForPly(game, 0));
            Assert.Contains("InitialBoardPosition", ex.Message);
        }

        [Fact]
        public void GetBoardPositionForPly_WhenPreviousPlyPositionMissing_ThrowsInvalidOperationException()
        {
            var game = new Game
            {
                Name = "G",
                InitialBoardPosition = sut.GetStartingBoardPosition(),
                Plies = new Dictionary<int, Ply>
                {
                    { 0, new Ply { RawMove = "e4", Colour = Colour.W } },
                    { 1, new Ply { RawMove = "e5", Colour = Colour.B } }
                },
                BoardPositions = new Dictionary<int, BoardPosition>() // no [0], so ply 1 has no previous
            };

            var ex = Assert.Throws<InvalidOperationException>(() => sut.GetBoardPositionForPly(game, 1));
            Assert.Contains("Previous board position", ex.Message);
        }
    }
}
