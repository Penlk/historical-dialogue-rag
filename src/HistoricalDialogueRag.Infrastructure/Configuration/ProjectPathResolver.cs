namespace HistoricalDialogueRag.Infrastructure.Configuration;

public static class ProjectPathResolver
{
    public static string Resolve(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is required.", nameof(path));

        if (Path.IsPathFullyQualified(path))
            return Path.GetFullPath(path);

        var root = FindRepositoryRoot() ?? Directory.GetCurrentDirectory();

        return Path.GetFullPath(Path.Combine(root, path));
    }

    private static string? FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            if (directory.GetFiles("*.sln").Length > 0)
                return directory.FullName;

            if (Directory.Exists(Path.Combine(directory.FullName, "src")) &&
                Directory.Exists(Path.Combine(directory.FullName, "data")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}