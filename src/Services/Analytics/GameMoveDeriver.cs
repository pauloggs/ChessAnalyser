using System.Numerics;
using Interfaces.Analytics;
using Interfaces.DTO;

namespace Services.Analytics;

public sealed class GameMoveDeriver : IGameMoveDeriver
{
    private static readonly char[] PieceTypes = ['P', 'N', 'B', 'R', 'Q', 'K'];

    public GameMoveFact Derive(int gameId, int plyIndex, BoardPosition previous, BoardPosition current)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        var movingSide = plyIndex % 2 == 0 ? 'W' : 'B';
        var enemySide = movingSide == 'W' ? 'B' : 'W';

        var friendly = BuildDiff(previous, current, movingSide);
        var enemy = BuildDiff(previous, current, enemySide);

        if (TryDeriveCastling(gameId, plyIndex, movingSide, friendly, enemy, out var castling))
            return castling;

        var totalFriendlyRemoved = friendly.Sum(d => d.RemovedCount);
        var totalFriendlyAdded = friendly.Sum(d => d.AddedCount);
        if (totalFriendlyRemoved != 1 || totalFriendlyAdded != 1)
            throw new InvalidOperationException(
                $"Unable to derive move shape at ply {plyIndex}: removed={totalFriendlyRemoved}, added={totalFriendlyAdded}.");

        var removed = friendly.Single(d => d.RemovedCount == 1);
        var added = friendly.Single(d => d.AddedCount == 1);

        var totalEnemyRemoved = enemy.Sum(d => d.RemovedCount);
        if (totalEnemyRemoved > 1)
            throw new InvalidOperationException($"Unexpected multi-capture at ply {plyIndex}.");

        var captured = totalEnemyRemoved == 1 ? enemy.Single(d => d.RemovedCount == 1).Piece : (char?)null;

        char? promotion = null;
        if (removed.Piece == 'P' && added.Piece != 'P')
        {
            if (added.Piece is not ('N' or 'B' or 'R' or 'Q'))
                throw new InvalidOperationException($"Invalid promotion piece '{added.Piece}' at ply {plyIndex}.");
            promotion = added.Piece;
        }
        else if (removed.Piece != added.Piece)
        {
            throw new InvalidOperationException(
                $"Derived piece mismatch at ply {plyIndex}: removed {removed.Piece}, added {added.Piece}.");
        }

        return new GameMoveFact
        {
            GameId = gameId,
            PlyIndex = plyIndex,
            MovingSide = movingSide,
            FromSquare = checked((byte)removed.RemovedSquare),
            ToSquare = checked((byte)added.AddedSquare),
            MovedPiece = removed.Piece,
            CapturedPiece = captured,
            PromotionPiece = promotion,
            IsCastlingKingside = false,
            IsCastlingQueenside = false
        };
    }

    private static bool TryDeriveCastling(
        int gameId, int plyIndex, char movingSide, IReadOnlyList<PieceDiff> friendly, IReadOnlyList<PieceDiff> enemy,
        out GameMoveFact move)
    {
        move = null!;

        var king = friendly.Single(d => d.Piece == 'K');
        var rook = friendly.Single(d => d.Piece == 'R');
        if (king.RemovedCount != 1 || king.AddedCount != 1 || rook.RemovedCount != 1 || rook.AddedCount != 1)
            return false;
        if (friendly.Sum(d => d.RemovedCount) != 2 || friendly.Sum(d => d.AddedCount) != 2)
            return false;
        if (enemy.Sum(d => d.RemovedCount) != 0)
            return false;

        var from = king.RemovedSquare;
        var to = king.AddedSquare;
        var deltaFile = Math.Abs((to % 8) - (from % 8));
        if (deltaFile != 2)
            return false;

        var kingside = (to % 8) > (from % 8);
        move = new GameMoveFact
        {
            GameId = gameId,
            PlyIndex = plyIndex,
            MovingSide = movingSide,
            FromSquare = checked((byte)from),
            ToSquare = checked((byte)to),
            MovedPiece = 'K',
            CapturedPiece = null,
            PromotionPiece = null,
            IsCastlingKingside = kingside,
            IsCastlingQueenside = !kingside
        };

        return true;
    }

    private static IReadOnlyList<PieceDiff> BuildDiff(BoardPosition previous, BoardPosition current, char side)
    {
        var list = new List<PieceDiff>(PieceTypes.Length);
        foreach (var piece in PieceTypes)
        {
            var key = $"{side}{piece}";
            var prev = Get(previous, key);
            var curr = Get(current, key);

            var removedBits = prev & ~curr;
            var addedBits = curr & ~prev;

            list.Add(new PieceDiff(
                piece,
                BitOperations.PopCount(removedBits),
                BitOperations.PopCount(addedBits),
                removedBits == 0 ? -1 : BitOperations.TrailingZeroCount(removedBits),
                addedBits == 0 ? -1 : BitOperations.TrailingZeroCount(addedBits)));
        }

        return list;
    }

    private static ulong Get(BoardPosition board, string key) =>
        board.PiecePositions.TryGetValue(key, out var bits) ? bits : 0UL;

    private sealed record PieceDiff(char Piece, int RemovedCount, int AddedCount, int RemovedSquare, int AddedSquare);
}
