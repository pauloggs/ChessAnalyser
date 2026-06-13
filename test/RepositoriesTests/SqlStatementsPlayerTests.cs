using Repositories;

namespace RepositoriesTests;

public class SqlStatementsPlayerTests
{
    [Fact]
    public void GetPlayers_SelectsIdentityColumnsOnly()
    {
        Assert.Contains("Id", SqlStatements.GetPlayers, StringComparison.Ordinal);
        Assert.Contains("Surname", SqlStatements.GetPlayers, StringComparison.Ordinal);
        Assert.Contains("Forenames", SqlStatements.GetPlayers, StringComparison.Ordinal);
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
