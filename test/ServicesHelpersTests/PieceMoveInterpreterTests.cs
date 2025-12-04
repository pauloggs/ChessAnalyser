using Interfaces;
using Interfaces.DTO;
using Moq;
using Services;
using Services.Helpers;
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
            IRankAndFileHelper rankAndFileHelper = new RankAndFileHelper();
            IPieceSourceFinderService pieceSourceFinderService = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper);
            var pieceMoveInterpreter = new PieceMoveInterpreter(sourceSquareHelper, pieceSourceFinderService);

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
            var rankAndFileHelper = new RankAndFileHelper();
            IPieceSourceFinderService pieceSourceFinderService = new PieceSourceFinderService(sourceSquareHelperMock.Object, rankAndFileHelper);
            var pieceMoveInterpreter = new PieceMoveInterpreter(sourceSquareHelperMock.Object, pieceSourceFinderService);
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

            var rankAndFileHelper = new RankAndFileHelper();

            IPieceSourceFinderService pieceSourceFinderService = new PieceSourceFinderService(sourceSquareHelperMock.Object, rankAndFileHelper);


            // Instantiate the SUT (PieceMoveInterpreter) with the ISourceSquareHelper MOCK
            var pieceMoveInterpreter = new PieceMoveInterpreter(sourceSquareHelperMock.Object, pieceSourceFinderService);

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

        [Fact]
        public void GetSourceSquare_KnightMove_TwoPossibleSources_DisambiguationRequired()
        {
            // Scenario: White knight on B1 and G1 can both move to D2. 
            // Move is "Nbd2" - specifies the B file.

            // Arrange
            var sourceSquareHelperMock = new Mock<ISourceSquareHelper>();
            var rankAndFileHelper = new RankAndFileHelper();
            IPieceSourceFinderService pieceSourceFinderService = new PieceSourceFinderService(sourceSquareHelperMock.Object, rankAndFileHelper);
            var pieceMoveInterpreter = new PieceMoveInterpreter(sourceSquareHelperMock.Object, pieceSourceFinderService);

            // Expected Source Square: B1 (Rank 0, File 1, Index 1)
            int expectedSourceSquareIndex = 1;
            int sourceRank = 0;
            int sourceFile = 1;

            var ply = new Ply
            {
                RawMove = "Nbd2",
                DestinationFile = 3, // 'd' file index
                DestinationRank = 1, // '2' rank index
                Piece = Constants.Pieces['N'],
                Colour = Colour.W
            };

            // 1. Mock GetSourceRankAndOrFile to return the specified 'b' file index (1)
            sourceSquareHelperMock
                .Setup(s => s.GetSourceRankAndOrFile(ply.RawMove))
                .Returns((Constants.MoveNotFound, 1)); // Specified File: 1, Specified Rank: -1

            // 2. Mock GetSourceSquare to return the expected index for B1
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
                    // IMPORTANT: Also return the *other* possible square (G1 -> R0, F6, Index 6) as a valid result
                    if (r == 0 && f == 6)
                    {
                        return 6; // Return index 6 for G1, but the interpreter should ignore this due to disambiguation
                    }
                    return -1;
                });

            // Act
            var sourceSquare = pieceMoveInterpreter.GetSourceSquare(new BoardPosition(), ply);

            // Assert
            // The interpreter MUST return B1 (index 1) because the 'b' file was specified in RawMove
            Assert.Equal(expectedSourceSquareIndex, sourceSquare);
        }

        [Fact]
        public void GetSourceSquare_KnightMove_NoPieceFound_ReturnsMoveNotFound()
        {
            // Scenario: Try to move a knight to D4 ("Nd4"), but no white knights are on valid source squares.

            // Arrange
            var sourceSquareHelperMock = new Mock<ISourceSquareHelper>();
            var rankAndFileHelper = new RankAndFileHelper();
            IPieceSourceFinderService pieceSourceFinderService = new PieceSourceFinderService(sourceSquareHelperMock.Object, rankAndFileHelper);
            var pieceMoveInterpreter = new PieceMoveInterpreter(sourceSquareHelperMock.Object, pieceSourceFinderService);

            var ply = new Ply
            {
                RawMove = "Nd4",
                DestinationFile = 3, // 'd'
                DestinationRank = 3, // '4'
                Piece = Constants.Pieces['N'],
                Colour = Colour.W
            };

            // Configure mock to return -1 for ALL calls (simulating an empty board)
            sourceSquareHelperMock
                .Setup(m => m.GetSourceSquare(
                    It.IsAny<BoardPosition>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Piece>(), It.IsAny<Colour>()))
                .Returns(-1);

            // Act
            var sourceSquare = pieceMoveInterpreter.GetSourceSquare(new BoardPosition(), ply);

            // Assert
            Assert.Equal(Constants.MoveNotFound, sourceSquare);
            Assert.Equal(-1, sourceSquare);
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
            IRankAndFileHelper rankAndFileHelper = new RankAndFileHelper();
            IPieceSourceFinderService pieceSourceFinderService = new PieceSourceFinderService(sourceSquareHelper, rankAndFileHelper);
            var pieceMoveInterpreter = new PieceMoveInterpreter(sourceSquareHelper, pieceSourceFinderService);

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
