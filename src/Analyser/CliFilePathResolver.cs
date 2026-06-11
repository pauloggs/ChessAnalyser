namespace Analyser;

/// <summary>
/// Resolves CLI file paths so repo-relative paths work when <c>dotnet run --project src/Analyser</c>
/// sets the working directory to the project folder.
/// </summary>
internal static class CliFilePathResolver
{
    internal static string Resolve(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (Path.IsPathRooted(path))
            return Path.GetFullPath(path);

        var fromCwd = Path.GetFullPath(path);
        if (File.Exists(fromCwd))
            return fromCwd;

        var fromRepo = Path.GetFullPath(Path.Combine(FindRepoRoot(), path));
        return File.Exists(fromRepo) ? fromRepo : fromCwd;
    }

    private static string FindRepoRoot()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var dir = new DirectoryInfo(start);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "ChessAnalyser.sln")))
                    return dir.FullName;
                dir = dir.Parent;
            }
        }

        return Directory.GetCurrentDirectory();
    }
}
