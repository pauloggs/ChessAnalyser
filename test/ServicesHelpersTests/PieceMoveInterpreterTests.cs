using Interfaces;
using Interfaces.DTO;
using Moq;
using Services;
using Services.Helpers;
using System.Diagnostics;
using static Interfaces.Constants;

namespace ServicesHelpersTests
{
    public class PieceMoveInterpreterTests
    {


        [Fact]
        public void GetSourceSquare_KnightMove_ReturnsCorrectSourceSquare()
        {
            // Arrange
            var bitBoardManipulatorMock = new Mock<IBitBoardManipulator>();
            ISourceSquareHelper sourceSquareHelper = new SourceSquareHelper(bitBoardManipulatorMock.Object);
            var pieceMoveInterpreter = new PieceMoveInterpreter(sourceSquareHelper);

            int expectedSourceSquareIndex = 30; // g4
            int expectedSourceRank = 3;
            int expectedSourceFile = 6;

            var previousBoardPosition = new BoardPosition();

            var ply = new Ply
            {
                RawMove = "Nf6",
                DestinationFile = 5, // 'f'
                DestinationRank = 5, // '6'
                Piece = Constants.Pieces['N'],
                Colour = Colour.W
            };

            // --- FIX ORDERING ---

            // 1. Set up the DEFAULT behavior FIRST: return FALSE for all calls
            bitBoardManipulatorMock
                .Setup(m => m.ReadSquare(
                    It.IsAny<BoardPosition>(), It.IsAny<Piece>(), It.IsAny<Colour>(),
                    It.IsAny<int>(), It.IsAny<int>()
                ))
                .Returns(false);

            // 2. Set up the SPECIFIC OVERRIDE SECOND: return TRUE only for the target g4 (R3, F6)
            bitBoardManipulatorMock
                .Setup(m => m.ReadSquare(
                    It.IsAny<BoardPosition>(), It.IsAny<Piece>(), It.IsAny<Colour>(),
                    It.Is<int>(rank => rank == expectedSourceRank),
                    It.Is<int>(file => file == expectedSourceFile)
                ))
                .Returns(true);
            // --- END FIX ORDERING ---


            // Act
            var sourceSquare = pieceMoveInterpreter.GetSourceSquare(previousBoardPosition, ply);

            // Assert
            Assert.Equal(expectedSourceSquareIndex, sourceSquare); // Expect index 30
        }

        [Theory]
        // (Expected Index, Source Rank Standard, Source File Standard, Dest Rank Standard, Dest File Standard)
        [InlineData(0, 0, 0, 2, 1)]
        [InlineData(63, 7, 7, 5, 6)]
        [InlineData(48, 6, 0, 4, 1)]
        [InlineData(28, 3, 4, 5, 3)]

        public void GetSourceSquare_KnightMove_Theory_ReturnsCorrectSourceSquares_V3(
        int expectedSourceSquareIndex,
        int sourceRank,
        int sourceFile,
        int destRank,
        int destFile)
        {
            // Arrange
            var sourceSquareHelperMock = new Mock<ISourceSquareHelper>();
            var pieceMoveInterpreter = new PieceMoveInterpreter(sourceSquareHelperMock.Object);
            var previousBoardPosition = new BoardPosition();

            var ply = new Ply
            {
                RawMove = "TestMove", // No disambiguation
                DestinationFile = destFile,
                DestinationRank = destRank,
                Piece = Constants.Pieces['N'],
                Colour = Colour.W
            };

            // 1. Mock GetSourceRankAndOrFile to return "not specified" so the RankAndFileHelper check passes
            sourceSquareHelperMock
                .Setup(s => s.GetSourceRankAndOrFile(It.IsAny<string>()))
                .Returns((Constants.MoveNotFound, Constants.MoveNotFound)); // Returns (-1, -1)

            // 2. Mock GetSourceSquare to return the expected index when the coordinates match
            sourceSquareHelperMock
                .Setup(m => m.GetSourceSquare(
                    It.IsAny<BoardPosition>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Piece>(), It.IsAny<Colour>()))
                .Returns((BoardPosition bp, int r, int f, Piece p, Colour c) =>
                {
                    if (r == sourceRank && f == sourceFile)
                    {
                        return expectedSourceSquareIndex;
                    }
                    return -1;
                });

            // Act
            var sourceSquare = pieceMoveInterpreter.GetSourceSquare(previousBoardPosition, ply);

            // Assert
            Assert.Equal(expectedSourceSquareIndex, sourceSquare);
        }

        [Fact]
        public void GetSourceSquare_KnightMove_DisambiguationByFile_OnlyChecksSpecifiedFile()
        {
            // Arrange
            // Mock the deepest dependency (Bitboard Manipulator)
            var bitBoardManipulatorMock = new Mock<IBitBoardManipulator>();

            // Mock the helper INTERFACE itself (do not pass constructor args here)
            var sourceSquareHelperMock = new Mock<ISourceSquareHelper>();

            // Instantiate the SUT (PieceMoveInterpreter) with the ISourceSquareHelper MOCK
            var pieceMoveInterpreter = new PieceMoveInterpreter(sourceSquareHelperMock.Object);

            // Scenario: Nbd2 (Knight from 'b' file to d2). Source square B1 (R0, F1)
            int expectedSourceSquareIndex = 1;
            int specifiedSourceFile = 1; // 'b' file index

            var previousBoardPosition = new BoardPosition();

            var ply = new Ply
            {
                RawMove = "Nbd2",
                DestinationFile = 3, // 'd'
                DestinationRank = 1, // '2'
                Piece = Constants.Pieces['N'],
                Colour = Colour.W
            };

            // Setup 1: Configure the GetSourceRankAndOrFile method on the ISourceSquareHelper MOCK
            sourceSquareHelperMock
                .Setup(s => s.GetSourceRankAndOrFile(ply.RawMove))
                .Returns((Constants.MoveNotFound, specifiedSourceFile)); // Returns Rank=-1, File=1

            // Setup 2: Configure the GetSourceSquare method on the ISourceSquareHelper MOCK
            // This setup must define the behavior of the helper's internal logic using a callback
            // to determine if the piece is "found" at specific coordinates.

            sourceSquareHelperMock
                .Setup(m => m.GetSourceSquare(
                    It.IsAny<BoardPosition>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Piece>(), It.IsAny<Colour>()))
                .Returns((BoardPosition bp, int r, int f, Piece p, Colour c) =>
                {
                    // Simulate the internal logic of SourceSquareHelper.GetSourceSquare
                    // (Note: We skip using bitBoardManipulatorMock here to simplify this test)
                    if (r == 0 && f == 1) // Only return the square index if coordinates match B1
                    {
                        return 1; // Return the index of B1
                    }
                    return -1;
                });

            // Act
            var sourceSquare = pieceMoveInterpreter.GetSourceSquare(previousBoardPosition, ply);

            // Assert
            Assert.Equal(expectedSourceSquareIndex, sourceSquare);
        }

        // Reusable setup method to make the Theory cleaner
        private int ExecuteKnightMoveTest(
            int expectedSourceSquareIndex,
            int sourceRank,
            int sourceFile,
            int destRank,
            int destFile)
        {
            var bitBoardManipulatorMock = new Mock<IBitBoardManipulator>();
            // Use the real helper, injecting the mock manipulator
            ISourceSquareHelper sourceSquareHelper = new SourceSquareHelper(bitBoardManipulatorMock.Object);
            var pieceMoveInterpreter = new PieceMoveInterpreter(sourceSquareHelper);

            var previousBoardPosition = new BoardPosition();

            var ply = new Ply
            {
                RawMove = "TestMove", // RawMove isn't used for disambiguation in these cases
                DestinationFile = destFile,
                DestinationRank = destRank,
                Piece = Constants.Pieces['N'],
                Colour = Colour.W
            };

            // 1. Set up the DEFAULT behavior FIRST: return FALSE for all calls
            bitBoardManipulatorMock
                .Setup(m => m.ReadSquare(It.IsAny<BoardPosition>(), It.IsAny<Piece>(), It.IsAny<Colour>(),
                    It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            // 2. Set up the SPECIFIC OVERRIDE SECOND: return TRUE only for the target source square
            bitBoardManipulatorMock
                .Setup(m => m.ReadSquare(It.IsAny<BoardPosition>(), It.IsAny<Piece>(), It.IsAny<Colour>(),
                    It.Is<int>(r => r == sourceRank),
                    It.Is<int>(f => f == sourceFile)
                ))
                .Returns(true);

            // Act
            return pieceMoveInterpreter.GetSourceSquare(previousBoardPosition, ply);
        }

    }
}
