using System.Data;
using Interfaces.DTO;
using Microsoft.Data.SqlClient;
using Services.PlayerMetadata;

namespace Migrations.Seeding;

/// <summary>
/// Streams the local FIDE rating list into <c>Ref.FidePlayer</c> when the catalog is empty.
/// </summary>
internal static class SeedRefFidePlayerRunner
{
    private const int BulkBatchSize = 5000;

    internal static async Task<int> RunIfNeededAsync(
        string connectionString,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var existing = await CountRowsAsync(connection, cancellationToken).ConfigureAwait(false);
        if (existing > 0)
        {
            Console.WriteLine("Ref.FidePlayer already contains {0} rows; skipping FIDE catalog seed.", existing);
            return 0;
        }

        if (!File.Exists(filePath))
        {
            Console.WriteLine(
                "FIDE catalog file not found at '{0}'. Ref.FidePlayer remains empty until the file is present and migrations run again.",
                filePath);
            return 0;
        }

        Console.WriteLine("Seeding Ref.FidePlayer from {0} ...", filePath);
        var imported = await ImportFileAsync(connection, filePath, cancellationToken).ConfigureAwait(false);
        Console.WriteLine("Seeded {0} rows into Ref.FidePlayer.", imported);
        return imported;
    }

    internal static async Task<bool> CatalogHasRowsAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return await CountRowsAsync(connection, cancellationToken).ConfigureAwait(false) > 0;
    }

    private static async Task<int> CountRowsAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand("SELECT COUNT(1) FROM Ref.FidePlayer;", connection);
        var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(result);
    }

    private static async Task<int> ImportFileAsync(
        SqlConnection connection,
        string filePath,
        CancellationToken cancellationToken)
    {
        var batch = new List<FidePlayerRecord>(BulkBatchSize);
        var imported = 0;

        await foreach (var line in ReadLinesAsync(filePath, cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!FideRatingListReader.TryParseLine(line, out var record))
                continue;

            batch.Add(record);
            if (batch.Count < BulkBatchSize)
                continue;

            await BulkInsertAsync(connection, batch, cancellationToken).ConfigureAwait(false);
            imported += batch.Count;
            batch.Clear();

            if (imported % 100_000 == 0)
                Console.WriteLine("  ... {0} FIDE rows inserted", imported);
        }

        if (batch.Count > 0)
        {
            await BulkInsertAsync(connection, batch, cancellationToken).ConfigureAwait(false);
            imported += batch.Count;
        }

        return imported;
    }

    private static async Task BulkInsertAsync(
        SqlConnection connection,
        IReadOnlyList<FidePlayerRecord> records,
        CancellationToken cancellationToken)
    {
        using var table = CreateDataTable(records);
        using var bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = "Ref.FidePlayer",
            BatchSize = BulkBatchSize
        };
        bulkCopy.ColumnMappings.Add("FideId", "FideId");
        bulkCopy.ColumnMappings.Add("Surname", "Surname");
        bulkCopy.ColumnMappings.Add("Forenames", "Forenames");
        bulkCopy.ColumnMappings.Add("Federation", "Federation");
        bulkCopy.ColumnMappings.Add("Sex", "Sex");
        bulkCopy.ColumnMappings.Add("FideTitle", "FideTitle");
        bulkCopy.ColumnMappings.Add("BirthYear", "BirthYear");

        await bulkCopy.WriteToServerAsync(table, cancellationToken).ConfigureAwait(false);
    }

    private static DataTable CreateDataTable(IReadOnlyList<FidePlayerRecord> records)
    {
        var table = new DataTable();
        table.Columns.Add("FideId", typeof(int));
        table.Columns.Add("Surname", typeof(string));
        table.Columns.Add("Forenames", typeof(string));
        table.Columns.Add("Federation", typeof(string));
        table.Columns.Add("Sex", typeof(string));
        table.Columns.Add("FideTitle", typeof(string));
        table.Columns.Add("BirthYear", typeof(short));

        foreach (var record in records)
        {
            table.Rows.Add(
                record.FideId,
                record.Surname,
                record.Forenames,
                record.Federation is null ? DBNull.Value : record.Federation,
                record.Sex is null ? DBNull.Value : record.Sex,
                record.Title is null ? DBNull.Value : record.Title,
                record.BirthYear is null ? DBNull.Value : record.BirthYear);
        }

        return table;
    }

    private static async IAsyncEnumerable<string> ReadLinesAsync(
        string filePath,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(filePath);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null)
                yield break;
            yield return line;
        }
    }
}
