using Services.Helpers;

namespace ServicesTests.Helpers;

public class RepoRootLocatorTests
{
    [Fact]
    public void Find_LocatesRepoRootFromSolutionFile()
    {
        var repoRoot = RepoRootLocator.Find();
        Assert.True(File.Exists(Path.Combine(repoRoot, RepoRootLocator.SolutionFileName)));
    }

    [Fact]
    public void ResolveRelativePath_UsesRepoRoot_NotHostWorkingDirectory()
    {
        var repoRoot = RepoRootLocator.Find();
        var resolved = RepoRootLocator.ResolveRelativePath("data/fide/players_list_foa.txt", repoRoot);

        Assert.Equal(
            Path.GetFullPath(Path.Combine(repoRoot, "data", "fide", "players_list_foa.txt")),
            resolved);
    }
}
