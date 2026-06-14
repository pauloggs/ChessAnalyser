namespace Services.Helpers;

/// <summary>
/// Locates the repository root so config paths like <c>data/fide/...</c> resolve consistently
/// whether the host runs from repo root, <c>src/Analyser</c>, or <c>bin/Debug</c>.
/// </summary>
public static class RepoRootLocator
{
    public const string SolutionFileName = "ChessAnalyser.sln";

    public static string Find(string? explicitRoot = null)
    {
        if (!string.IsNullOrWhiteSpace(explicitRoot))
            return Path.GetFullPath(explicitRoot);

        foreach (var start in CandidateStartDirectories())
        {
            var found = FindFrom(start);
            if (found is not null)
                return found;
        }

        return Directory.GetCurrentDirectory();
    }

    public static string ResolveRelativePath(string relativeOrAbsolutePath, string? explicitRoot = null)
    {
        var configured = relativeOrAbsolutePath.Trim();
        if (Path.IsPathRooted(configured))
            return Path.GetFullPath(configured);

        return Path.GetFullPath(Path.Combine(Find(explicitRoot), configured));
    }

    private static IEnumerable<string> CandidateStartDirectories()
    {
        yield return Directory.GetCurrentDirectory();

        var dir = Normalize(AppContext.BaseDirectory);
        while (!string.IsNullOrEmpty(dir))
        {
            yield return dir;
            dir = Normalize(Directory.GetParent(dir)?.FullName);
        }
    }

    private static string? FindFrom(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, SolutionFileName)))
                return dir.FullName;

            dir = dir.Parent;
        }

        return null;
    }

    private static string Normalize(string? path) =>
        string.IsNullOrWhiteSpace(path)
            ? string.Empty
            : path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
