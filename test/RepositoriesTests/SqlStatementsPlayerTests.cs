using Repositories;

namespace RepositoriesTests;

public class SqlStatementsPlayerTests
{
    [Fact]
    public void GetPlayers_IncludesMetadataForeignKeys()
    {
        Assert.Contains("WorldChampionId", SqlStatements.GetPlayers, StringComparison.Ordinal);
        Assert.Contains("FidePlayerId", SqlStatements.GetPlayers, StringComparison.Ordinal);
    }

    [Fact]
    public void UpdatePlayerMetadataLinks_UpdatesForeignKeys()
    {
        var sql = SqlStatements.UpdatePlayerMetadataLinks;
        Assert.Contains("UPDATE App.Player", sql, StringComparison.Ordinal);
        Assert.Contains("WorldChampionId", sql, StringComparison.Ordinal);
        Assert.Contains("FidePlayerId", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void GetPlayersBySurname_SelectsIdentityColumnsOnly()
    {
        Assert.Contains("Surname", SqlStatements.GetPlayersBySurname, StringComparison.Ordinal);
        Assert.Contains("Forenames", SqlStatements.GetPlayersBySurname, StringComparison.Ordinal);
    }

    [Fact]
    public void InsertPlayer_InsertsSurnameAndForenames()
    {
        var sql = SqlStatements.InsertPlayer;
        Assert.Contains("INSERT INTO App.Player (Surname, Forenames)", sql, StringComparison.Ordinal);
        Assert.Contains("@Surname", sql, StringComparison.Ordinal);
        Assert.Contains("@Forenames", sql, StringComparison.Ordinal);
    }
}
