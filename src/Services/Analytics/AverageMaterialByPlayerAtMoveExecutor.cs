using Interfaces.Analytics;
using Repositories;

namespace Services.Analytics;

/// <summary>
/// Compares one player's average material at a full move against another player or all players.
/// </summary>
public sealed class AverageMaterialByPlayerAtMoveExecutor(IChessRepository repository) : IMetricExecutor
{
    private const int DefaultMoveNumber = 1;
    private const string DefaultColourMode = "Any";

    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public string MetricKey => "AverageMaterialByPlayerAtMove";

    public async Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query.PlayerASurname))
            throw new ArgumentException("playerASurname is required for AverageMaterialByPlayerAtMove.", nameof(query));

        var moveNumber = query.MoveNumber ?? DefaultMoveNumber;
        var plyIndex = MoveNumberToPlyIndex(moveNumber);
        var colourMode = NormalizeColourMode(query.PlayerColour);

        var rows = await _repository
            .GetPlayerMaterialAveragesAtPlyAsync(query, moveNumber, plyIndex, colourMode, cancellationToken)
            .ConfigureAwait(false);

        if (!rows.Any(r => string.Equals(r.Series, "PlayerA", StringComparison.OrdinalIgnoreCase)))
            return AnalyticsTableResult.Empty("Series", "Player", "Colour", "MoveNumber", "PlyIndex", "AvgMaterial", "PositionCount");

        IReadOnlyList<object?> Row(PlayerMaterialAverageRow r) =>
            new object?[]
            {
                r.Series,
                FormatPlayer(r.PlayerSurname, r.PlayerForenames),
                r.Colour,
                r.MoveNumber,
                r.PlyIndex,
                r.AvgMaterial,
                r.PositionCount
            };

        return new AnalyticsTableResult
        {
            ColumnNames = ["Series", "Player", "Colour", "MoveNumber", "PlyIndex", "AvgMaterial", "PositionCount"],
            Rows = rows.Select(r => (IReadOnlyList<object?>)Row(r).ToList()).ToList()
        };
    }

    public static int MoveNumberToPlyIndex(int moveNumber)
    {
        if (moveNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(moveNumber), "moveNumber must be >= 1.");

        return (moveNumber * 2) - 1;
    }

    public static string NormalizeColourMode(string? colour)
    {
        if (string.IsNullOrWhiteSpace(colour))
            return DefaultColourMode;

        var trimmed = colour.Trim();
        if (string.Equals(trimmed, "Any", StringComparison.OrdinalIgnoreCase))
            return "Any";
        if (string.Equals(trimmed, "White", StringComparison.OrdinalIgnoreCase))
            return "White";
        if (string.Equals(trimmed, "Black", StringComparison.OrdinalIgnoreCase))
            return "Black";

        throw new ArgumentException("playerColour must be Any, White, or Black.", nameof(colour));
    }

    private static string FormatPlayer(string? surname, string? forenames)
    {
        if (string.IsNullOrWhiteSpace(surname))
            return "All players";

        var s = surname.Trim();
        var f = forenames?.Trim() ?? string.Empty;
        return f.Length == 0 ? s : $"{s}, {f}";
    }
}
