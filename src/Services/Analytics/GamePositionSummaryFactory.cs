using System.Numerics;
using Interfaces.Analytics;
using Interfaces.DTO;

namespace Services.Analytics;

public sealed class GamePositionSummaryFactory(IPieceValues pieceValues) : IGamePositionSummaryFactory
{
    private readonly IPieceValues _pieceValues = pieceValues ?? throw new ArgumentNullException(nameof(pieceValues));

    public GamePositionSummary Create(int gameId, int plyIndex, BoardPosition boardPosition)
    {
        ArgumentNullException.ThrowIfNull(boardPosition);
        ArgumentNullException.ThrowIfNull(boardPosition.PiecePositions);

        static short Pop(ulong b) => (short)BitOperations.PopCount(b);

        var wp = Pop(Get(boardPosition, "WP"));
        var wn = Pop(Get(boardPosition, "WN"));
        var wb = Pop(Get(boardPosition, "WB"));
        var wr = Pop(Get(boardPosition, "WR"));
        var wq = Pop(Get(boardPosition, "WQ"));
        var wk = Pop(Get(boardPosition, "WK"));

        var bp = Pop(Get(boardPosition, "BP"));
        var bn = Pop(Get(boardPosition, "BN"));
        var bb = Pop(Get(boardPosition, "BB"));
        var br = Pop(Get(boardPosition, "BR"));
        var bq = Pop(Get(boardPosition, "BQ"));
        var bk = Pop(Get(boardPosition, "BK"));

        var v = _pieceValues;
        var whiteMaterial = (short)(wp * v.ValueForPieceType('P')
            + wn * v.ValueForPieceType('N')
            + wb * v.ValueForPieceType('B')
            + wr * v.ValueForPieceType('R')
            + wq * v.ValueForPieceType('Q')
            + wk * v.ValueForPieceType('K'));

        var blackMaterial = (short)(bp * v.ValueForPieceType('P')
            + bn * v.ValueForPieceType('N')
            + bb * v.ValueForPieceType('B')
            + br * v.ValueForPieceType('R')
            + bq * v.ValueForPieceType('Q')
            + bk * v.ValueForPieceType('K'));

        return new GamePositionSummary
        {
            GameId = gameId,
            PlyIndex = plyIndex,
            WhiteMaterial = whiteMaterial,
            BlackMaterial = blackMaterial,
            WhitePawnCount = wp,
            WhiteKnightCount = wn,
            WhiteBishopCount = wb,
            WhiteRookCount = wr,
            WhiteQueenCount = wq,
            WhiteKingCount = wk,
            BlackPawnCount = bp,
            BlackKnightCount = bn,
            BlackBishopCount = bb,
            BlackRookCount = br,
            BlackQueenCount = bq,
            BlackKingCount = bk
        };
    }

    private static ulong Get(BoardPosition board, string key) =>
        board.PiecePositions.TryGetValue(key, out var bits) ? bits : 0UL;
}
