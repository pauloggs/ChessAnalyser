using Repositories;

namespace RepositoriesTests;

public class SqlStatementsPlayerTests
{
    [Fact]
    public void GetPlayers_SelectsFideMetadataColumns()
    {
        Assert.Contains("FideId", SqlStatements.GetPlayers, StringComparison.Ordinal);
        Assert.Contains("Federation", SqlStatements.GetPlayers, StringComparison.Ordinal);
        Assert.Contains("Sex", SqlStatements.GetPlayers, StringComparison.Ordinal);
        Assert.Contains("FideTitle", SqlStatements.GetPlayers, StringComparison.Ordinal);
        Assert.Contains("BirthYear", SqlStatements.GetPlayers, StringComparison.Ordinal);
    }

    [Fact]
    public void GetPlayersBySurname_SelectsFideMetadataColumns()
    {
        Assert.Contains("FideId", SqlStatements.GetPlayersBySurname, StringComparison.Ordinal);
        Assert.Contains("BirthYear", SqlStatements.GetPlayersBySurname, StringComparison.Ordinal);
    }

    [Fact]
    public void UpdatePlayerFideMetadata_UpdatesAllFideColumns()
    {
        var sql = SqlStatements.UpdatePlayerFideMetadata;
        Assert.Contains("FideId = @FideId", sql, StringComparison.Ordinal);
        Assert.Contains("Federation = @Federation", sql, StringComparison.Ordinal);
        Assert.Contains("Sex = @Sex", sql, StringComparison.Ordinal);
        Assert.Contains("FideTitle = @FideTitle", sql, StringComparison.Ordinal);
        Assert.Contains("BirthYear = @BirthYear", sql, StringComparison.Ordinal);
        Assert.Contains("WHERE Id = @Id", sql, StringComparison.Ordinal);
    }
}
