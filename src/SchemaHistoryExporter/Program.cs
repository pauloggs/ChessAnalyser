using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace SchemaHistoryExporter;

internal static class Program
{
    private static readonly Regex ScriptDateHeaderRegex = new(
        @"^\uFEFF?/\*{6}\s+Object:\s+.*?Script Date:.*?\*{6}/\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline);

    public static async Task<int> Main(string[] args)
    {
        var argv = args.ToList();
        var connectionString = GetArg(argv, "--connection-string");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            PrintUsage();
            return 1;
        }

        var repoRoot = GetRepoRoot(GetArg(argv, "--repo-root"));
        var outputRelative = GetArg(argv, "--output") ?? Path.Combine("src", "Migrations", "History", "current");
        var outputRoot = Path.GetFullPath(Path.Combine(repoRoot, outputRelative));

        var builder = new SqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
        {
            Console.Error.WriteLine("Connection string must include Initial Catalog (Database).");
            return 1;
        }

        ClearHistoryOutput(outputRoot);

        await using var sqlConnection = new SqlConnection(connectionString);
        await sqlConnection.OpenAsync();

        // In CI/Linux, SMO is more reliable when bound to an open SqlConnection
        // instead of parsing the raw connection string itself.
        var serverConnection = new ServerConnection(sqlConnection);
        var server = new Server(serverConnection);

        var db = server.Databases[builder.InitialCatalog];
        if (db == null)
        {
            Console.Error.WriteLine($"Database '{builder.InitialCatalog}' was not found on the server.");
            return 1;
        }

        var scripter = new Scripter(server)
        {
            Options =
            {
                AnsiFile = true,
                IncludeHeaders = true,
                ScriptDrops = false,
                WithDependencies = false,
                SchemaQualify = true,
                DriAll = true,
                Indexes = true,
                Triggers = true,
                NoCollation = false
            }
        };

        ScriptSmoObjects(scripter, Path.Combine(outputRoot, "tables"),
            db.Tables.Cast<Table>().Where(t => !t.IsSystemObject).Select(t => (t.Schema, t.Name, (SqlSmoObject)t)));

        ScriptSmoObjects(scripter, Path.Combine(outputRoot, "views"),
            db.Views.Cast<View>().Where(v => !v.IsSystemObject).Select(v => (v.Schema, v.Name, (SqlSmoObject)v)));

        ScriptSmoObjects(scripter, Path.Combine(outputRoot, "procedures"),
            db.StoredProcedures.Cast<StoredProcedure>().Where(p => !p.IsSystemObject)
                .Select(p => (p.Schema, p.Name, (SqlSmoObject)p)));

        ScriptSmoObjects(scripter, Path.Combine(outputRoot, "functions"),
            db.UserDefinedFunctions.Cast<UserDefinedFunction>().Where(f => !f.IsSystemObject)
                .Select(f => (f.Schema, f.Name, (SqlSmoObject)f)));

        ScriptSmoObjects(scripter, Path.Combine(outputRoot, "types"),
            db.UserDefinedTableTypes.Cast<UserDefinedTableType>().Select(t => (t.Schema, t.Name, (SqlSmoObject)t)));

        Console.WriteLine($"Exported schema history to {outputRoot}");
        return 0;
    }

    private static void PrintUsage()
    {
        Console.Error.WriteLine(
            "Usage: SchemaHistoryExporter --connection-string \"<sql>\" [--repo-root <path>] [--output <relativePath>]");
        Console.Error.WriteLine(
            "  Default --output: src/Migrations/History/current (relative to repo root)");
    }

    private static string? GetArg(IReadOnlyList<string> argv, string name)
    {
        for (var i = 0; i < argv.Count - 1; i++)
        {
            if (string.Equals(argv[i], name, StringComparison.OrdinalIgnoreCase))
                return argv[i + 1];
        }

        return null;
    }

    private static string GetRepoRoot(string? explicitRoot)
    {
        if (!string.IsNullOrWhiteSpace(explicitRoot))
            return Path.GetFullPath(explicitRoot);

        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "ChessAnalyser.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    private static void ClearHistoryOutput(string outputDir)
    {
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
            return;
        }

        foreach (var entry in Directory.EnumerateFileSystemEntries(outputDir, "*", SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileName(entry);
            if (string.Equals(name, ".gitkeep", StringComparison.OrdinalIgnoreCase))
                continue;
            if (Directory.Exists(entry))
                Directory.Delete(entry, recursive: true);
            else
                File.Delete(entry);
        }
    }

    private static string SafeFileName(string schema, string name)
    {
        var combined = $"{schema}.{name}";
        var invalid = Path.GetInvalidFileNameChars();
        return Regex.Replace(combined, $"[{Regex.Escape(new string(invalid))}]", "_") + ".sql";
    }

    private static void ScriptSmoObjects(Scripter scripter, string groupDir,
        IEnumerable<(string Schema, string Name, SqlSmoObject Obj)> items)
    {
        Directory.CreateDirectory(groupDir);
        foreach (var (schema, name, obj) in items.OrderBy(i => i.Schema, StringComparer.Ordinal)
                     .ThenBy(i => i.Name, StringComparer.Ordinal))
        {
            var scriptParts = scripter.Script(new SqlSmoObject[] { obj });
            if (scriptParts == null || scriptParts.Count == 0)
                continue;

            var sb = new StringBuilder();
            foreach (string? line in scriptParts)
            {
                if (line == null)
                    continue;
                sb.AppendLine(line);
            }

            var normalized = ScriptDateHeaderRegex.Replace(sb.ToString(), string.Empty)
                .Replace("\r\n", "\n")
                .TrimEnd('\n', '\r')
                .Replace("\n", Environment.NewLine);

            var body = normalized + Environment.NewLine;
            var path = Path.Combine(groupDir, SafeFileName(schema, name));
            File.WriteAllText(path, body, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
    }
}
