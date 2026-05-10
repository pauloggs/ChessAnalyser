using System.Diagnostics;
using Interfaces.DTO;

namespace Services.Analytics;

/// <summary>
/// In-memory timing for <see cref="AnalyticsMaterializationService"/> (deriver + summaries + orchestration, no SQL).
/// </summary>
public static class MaterializationPerfHarness
{
    public readonly record struct MaterializationPerfResult(
        int Iterations,
        double ElapsedMilliseconds,
        int SummaryRowsPerIteration,
        int MoveRowsPerIteration)
    {
        public int RowsWrittenPerIteration => SummaryRowsPerIteration + MoveRowsPerIteration;

        public double GamesPerSecond =>
            ElapsedMilliseconds <= 0 ? 0 : Iterations / (ElapsedMilliseconds / 1000.0);

        public double DerivedRowsPerSecond =>
            ElapsedMilliseconds <= 0 ? 0 : Iterations * RowsWrittenPerIteration / (ElapsedMilliseconds / 1000.0);
    }

    /// <summary>
    /// Runs repeated materialization for a fixed two-ply chain (initial + after 1.e4), with a no-op repository (no database I/O).
    /// </summary>
    public static async Task<MaterializationPerfResult> RunAsync(int iterations, CancellationToken cancellationToken = default)
    {
        if (iterations < 1)
            throw new ArgumentOutOfRangeException(nameof(iterations));

        const int summaryRowsPerIteration = 2;
        const int moveRowsPerIteration = 1;

        var repo = new MaterializationWriteOnlyNoOpRepository();
        var sut = new AnalyticsMaterializationService(
            new GameMoveDeriver(),
            new GamePositionSummaryFactory(new ClassicalPieceValues()),
            repo);

        var ordered = BuildTwoPlyWhiteE4();
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = await sut.MaterializeFromOrderedPliesAsync(1, ordered, cancellationToken).ConfigureAwait(false);
        }

        sw.Stop();
        return new MaterializationPerfResult(
            iterations,
            sw.Elapsed.TotalMilliseconds,
            summaryRowsPerIteration,
            moveRowsPerIteration);
    }

    private static List<(int PlyIndex, BoardPosition Position)> BuildTwoPlyWhiteE4()
    {
        var previous = EmptyBoard();
        Set(previous, "WP", Sq('e', 2));
        var current = Clone(previous);
        Move(current, "WP", Sq('e', 2), Sq('e', 4));
        return [(-1, previous), (0, current)];
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
