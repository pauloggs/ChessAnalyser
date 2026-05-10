using Interfaces.Analytics;
using Interfaces.DTO;
using Moq;
using Repositories;
using Services.Analytics;

namespace ServicesTests.Analytics;

public class AnalyticsMaterializationServiceTests
{
    [Fact]
    public async Task MaterializeAfterGamePersistedAsync_InitialOnly_PersistsOneSummaryAndNoMoves()
    {
        var repo = new Mock<IChessRepository>();
        IReadOnlyList<GameMoveFact>? moves = null;
        IReadOnlyList<GamePositionSummary>? summaries = null;
        repo.Setup(r => r.ReplaceGameMovesForGame(It.IsAny<int>(), It.IsAny<IReadOnlyList<GameMoveFact>>()))
            .Callback<int, IReadOnlyList<GameMoveFact>>((_, m) => moves = m)
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.ReplaceGamePositionSummariesForGame(It.IsAny<int>(), It.IsAny<IReadOnlyList<GamePositionSummary>>()))
            .Callback<int, IReadOnlyList<GamePositionSummary>>((_, s) => summaries = s)
            .Returns(Task.CompletedTask);

        var sut = new AnalyticsMaterializationService(
            new GameMoveDeriver(),
            new GamePositionSummaryFactory(new ClassicalPieceValues()),
            repo.Object);

        var initial = EmptyBoard();
        Set(initial, "WP", Sq('e', 2));
        var game = new Game { Name = "Test", InitialBoardPosition = initial, BoardPositions = new Dictionary<int, BoardPosition>() };

        await sut.MaterializeAfterGamePersistedAsync(game, 7);

        repo.Verify(r => r.ReplaceGamePositionSummariesForGame(7, It.IsAny<IReadOnlyList<GamePositionSummary>>()), Times.Once);
        repo.Verify(r => r.ReplaceGameMovesForGame(7, It.IsAny<IReadOnlyList<GameMoveFact>>()), Times.Once);
        Assert.NotNull(summaries);
        Assert.Single(summaries!);
        Assert.Equal(-1, summaries![0].PlyIndex);
        Assert.NotNull(moves);
        Assert.Empty(moves!);
    }

    [Fact]
    public async Task MaterializeAfterGamePersistedAsync_OneMove_PersistsOneMoveAndTwoSummaries()
    {
        var repo = new Mock<IChessRepository>();
        IReadOnlyList<GameMoveFact>? moves = null;
        IReadOnlyList<GamePositionSummary>? summaries = null;
        repo.Setup(r => r.ReplaceGameMovesForGame(It.IsAny<int>(), It.IsAny<IReadOnlyList<GameMoveFact>>()))
            .Callback<int, IReadOnlyList<GameMoveFact>>((_, m) => moves = m)
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.ReplaceGamePositionSummariesForGame(It.IsAny<int>(), It.IsAny<IReadOnlyList<GamePositionSummary>>()))
            .Callback<int, IReadOnlyList<GamePositionSummary>>((_, s) => summaries = s)
            .Returns(Task.CompletedTask);

        var sut = new AnalyticsMaterializationService(
            new GameMoveDeriver(),
            new GamePositionSummaryFactory(new ClassicalPieceValues()),
            repo.Object);

        var prev = EmptyBoard();
        Set(prev, "WP", Sq('e', 2));
        var curr = Clone(prev);
        Move(curr, "WP", Sq('e', 2), Sq('e', 4));

        var game = new Game
        {
            Name = "Test",
            BoardPositions = new Dictionary<int, BoardPosition> { [-1] = prev, [0] = curr }
        };

        await sut.MaterializeAfterGamePersistedAsync(game, 99);

        repo.Verify(r => r.ReplaceGamePositionSummariesForGame(99, It.IsAny<IReadOnlyList<GamePositionSummary>>()), Times.Once);
        repo.Verify(r => r.ReplaceGameMovesForGame(99, It.IsAny<IReadOnlyList<GameMoveFact>>()), Times.Once);
        Assert.NotNull(moves);
        Assert.Single(moves!);
        Assert.Equal(99, moves![0].GameId);
        Assert.Equal(0, moves[0].PlyIndex);
        Assert.NotNull(summaries);
        Assert.Equal(2, summaries!.Count);
        Assert.Equal(-1, summaries[0].PlyIndex);
        Assert.Equal(0, summaries[1].PlyIndex);
    }

    [Fact]
    public async Task MaterializeAfterGamePersistedAsync_NonContiguousPly_DoesNotCallReplace()
    {
        var repo = new Mock<IChessRepository>();
        var sut = new AnalyticsMaterializationService(
            new GameMoveDeriver(),
            new GamePositionSummaryFactory(new ClassicalPieceValues()),
            repo.Object);

        var a = EmptyBoard();
        Set(a, "WP", Sq('e', 2));
        var b = Clone(a);
        Move(b, "WP", Sq('e', 2), Sq('e', 4));

        var game = new Game
        {
            Name = "Test",
            BoardPositions = new Dictionary<int, BoardPosition> { [-1] = a, [2] = b }
        };

        await sut.MaterializeAfterGamePersistedAsync(game, 1);

        repo.Verify(r => r.ReplaceGamePositionSummariesForGame(It.IsAny<int>(), It.IsAny<IReadOnlyList<GamePositionSummary>>()), Times.Never);
        repo.Verify(r => r.ReplaceGameMovesForGame(It.IsAny<int>(), It.IsAny<IReadOnlyList<GameMoveFact>>()), Times.Never);
    }

    [Fact]
    public async Task MaterializeAfterGamePersistedAsync_DeriverThrows_DoesNotCallReplace()
    {
        var repo = new Mock<IChessRepository>();
        var deriver = new Mock<IGameMoveDeriver>();
        deriver.Setup(d => d.Derive(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<BoardPosition>(), It.IsAny<BoardPosition>()))
            .Throws(new InvalidOperationException("bad derive"));

        var sut = new AnalyticsMaterializationService(
            deriver.Object,
            new GamePositionSummaryFactory(new ClassicalPieceValues()),
            repo.Object);

        var prev = EmptyBoard();
        Set(prev, "WP", Sq('e', 2));
        var curr = Clone(prev);
        Move(curr, "WP", Sq('e', 2), Sq('e', 4));
        var game = new Game
        {
            Name = "Test",
            BoardPositions = new Dictionary<int, BoardPosition> { [-1] = prev, [0] = curr }
        };

        await sut.MaterializeAfterGamePersistedAsync(game, 1);

        repo.Verify(r => r.ReplaceGamePositionSummariesForGame(It.IsAny<int>(), It.IsAny<IReadOnlyList<GamePositionSummary>>()), Times.Never);
        repo.Verify(r => r.ReplaceGameMovesForGame(It.IsAny<int>(), It.IsAny<IReadOnlyList<GameMoveFact>>()), Times.Never);
    }

    private static BoardPosition EmptyBoard() => new();

    private static BoardPosition Clone(BoardPosition board) =>
        new()
        {
            PiecePositions = new Dictionary<string, ulong>(board.PiecePositions),
            EnPassantTargetFile = board.EnPassantTargetFile
        };

    private static void Set(BoardPosition board, string key, int square) =>
        board.PiecePositions[key] |= 1UL << square;

    private static void Remove(BoardPosition board, string key, int square) =>
        board.PiecePositions[key] &= ~(1UL << square);

    private static void Move(BoardPosition board, string key, int from, int to)
    {
        Remove(board, key, from);
        Set(board, key, to);
    }

    private static byte Sq(char file, int rank)
    {
        var fileIndex = char.ToLowerInvariant(file) - 'a';
        return checked((byte)((rank - 1) * 8 + fileIndex));
    }
}
