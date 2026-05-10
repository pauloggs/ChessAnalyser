using Repositories;

namespace RepositoriesTests;

/// <summary>
/// Guards analytics SQL shape without requiring a live database (integration tests optional separately).
/// </summary>
public class SqlStatementsAnalyticsTests
{
    [Fact]
    public void GameMove_Statements_ReferenceTableAndKeyColumns()
    {
        Assert.Contains("dbo.GameMove", SqlStatements.DeleteGameMovesForGame, StringComparison.Ordinal);
        Assert.Contains("dbo.GameMove", SqlStatements.InsertGameMove, StringComparison.Ordinal);
        Assert.Contains("MovingSide", SqlStatements.InsertGameMove, StringComparison.Ordinal);
        Assert.Contains("FromSquare", SqlStatements.InsertGameMove, StringComparison.Ordinal);
        Assert.Contains("ToSquare", SqlStatements.InsertGameMove, StringComparison.Ordinal);
        Assert.Contains("IsCastlingKingside", SqlStatements.InsertGameMove, StringComparison.Ordinal);
        Assert.Contains("ORDER BY PlyIndex", SqlStatements.GetGameMovesForGame, StringComparison.Ordinal);
    }

    [Fact]
    public void GamePositionSummary_Statements_ReferenceTableAndKeyColumns()
    {
        Assert.Contains("dbo.GamePositionSummary", SqlStatements.DeleteGamePositionSummariesForGame, StringComparison.Ordinal);
        Assert.Contains("dbo.GamePositionSummary", SqlStatements.InsertGamePositionSummary, StringComparison.Ordinal);
        Assert.Contains("WhiteMaterial", SqlStatements.InsertGamePositionSummary, StringComparison.Ordinal);
        Assert.Contains("BlackKingCount", SqlStatements.InsertGamePositionSummary, StringComparison.Ordinal);
        Assert.Contains("ORDER BY PlyIndex", SqlStatements.GetGamePositionSummariesForGame, StringComparison.Ordinal);
    }
}
