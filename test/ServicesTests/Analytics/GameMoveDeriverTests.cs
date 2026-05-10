using Interfaces.DTO;
using Services.Analytics;

namespace ServicesTests.Analytics;

public class GameMoveDeriverTests
{
    private readonly GameMoveDeriver _sut = new();

    [Fact]
    public void Derive_PawnPushE2E4_ReturnsExpectedFact()
    {
        var previous = EmptyBoard();
        Set(previous, "WP", Sq('e', 2));

        var current = Clone(previous);
        Move(current, "WP", Sq('e', 2), Sq('e', 4));

        var move = _sut.Derive(1, 0, previous, current);

        Assert.Equal('W', move.MovingSide);
        Assert.Equal('P', move.MovedPiece);
        Assert.Equal(Sq('e', 2), move.FromSquare);
        Assert.Equal(Sq('e', 4), move.ToSquare);
        Assert.Null(move.CapturedPiece);
        Assert.Null(move.PromotionPiece);
        Assert.False(move.IsCastlingKingside);
        Assert.False(move.IsCastlingQueenside);
    }

    [Fact]
    public void Derive_CaptureE4xD5_SetsCapturedPiece()
    {
        var previous = EmptyBoard();
        Set(previous, "WP", Sq('e', 4));
        Set(previous, "BP", Sq('d', 5));

        var current = Clone(previous);
        Move(current, "WP", Sq('e', 4), Sq('d', 5));
        Remove(current, "BP", Sq('d', 5));

        var move = _sut.Derive(7, 0, previous, current);

        Assert.Equal('P', move.MovedPiece);
        Assert.Equal('P', move.CapturedPiece);
        Assert.Equal(Sq('e', 4), move.FromSquare);
        Assert.Equal(Sq('d', 5), move.ToSquare);
    }

    [Fact]
    public void Derive_PromotionE7E8Q_SetsPromotionPiece()
    {
        var previous = EmptyBoard();
        Set(previous, "WP", Sq('e', 7));

        var current = Clone(previous);
        Remove(current, "WP", Sq('e', 7));
        Set(current, "WQ", Sq('e', 8));

        var move = _sut.Derive(3, 0, previous, current);

        Assert.Equal('P', move.MovedPiece);
        Assert.Equal('Q', move.PromotionPiece);
        Assert.Equal(Sq('e', 7), move.FromSquare);
        Assert.Equal(Sq('e', 8), move.ToSquare);
    }

    [Fact]
    public void Derive_WhiteKingsideCastle_SetsCastlingFlags()
    {
        var previous = EmptyBoard();
        Set(previous, "WK", Sq('e', 1));
        Set(previous, "WR", Sq('h', 1));

        var current = Clone(previous);
        Move(current, "WK", Sq('e', 1), Sq('g', 1));
        Move(current, "WR", Sq('h', 1), Sq('f', 1));

        var move = _sut.Derive(9, 0, previous, current);

        Assert.Equal('K', move.MovedPiece);
        Assert.True(move.IsCastlingKingside);
        Assert.False(move.IsCastlingQueenside);
        Assert.Equal(Sq('e', 1), move.FromSquare);
        Assert.Equal(Sq('g', 1), move.ToSquare);
    }

    [Fact]
    public void Derive_BlackMove_UsesBlackMovingSideForOddPly()
    {
        var previous = EmptyBoard();
        Set(previous, "BP", Sq('e', 7));

        var current = Clone(previous);
        Move(current, "BP", Sq('e', 7), Sq('e', 5));

        var move = _sut.Derive(11, 1, previous, current);

        Assert.Equal('B', move.MovingSide);
        Assert.Equal('P', move.MovedPiece);
        Assert.Equal(Sq('e', 7), move.FromSquare);
        Assert.Equal(Sq('e', 5), move.ToSquare);
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
