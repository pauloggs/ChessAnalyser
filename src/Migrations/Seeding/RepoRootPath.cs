namespace Migrations.Seeding;

internal static class RepoRootPath
{
    internal static string Resolve(string? configuredRelativePath = null)
    {
        var repoRoot = FindRepoRoot();
        if (string.IsNullOrWhiteSpace(configuredRelativePath))
            return repoRoot;

        return Path.IsPathRooted(configuredRelativePath)
            ? Path.GetFullPath(configuredRelativePath)
            : Path.GetFullPath(Path.Combine(repoRoot, configuredRelativePath));
    }

    internal static string FindRepoRoot()
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
