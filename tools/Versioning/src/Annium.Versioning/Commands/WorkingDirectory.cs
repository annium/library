using System.IO;

namespace Annium.Versioning.Commands;

internal static class WorkingDirectory
{
    /// <summary>
    /// Resolves the repository path a command works on: the configured directory, or the process's
    /// own when it was omitted. A path that does not exist is a mistyped invocation, not an empty
    /// repository, so it throws rather than reporting a version derived from nothing.
    /// </summary>
    /// <param name="configured">The directory passed on the command line, if any.</param>
    /// <returns>The absolute path of an existing directory.</returns>
    /// <exception cref="DirectoryNotFoundException">The resolved directory does not exist.</exception>
    public static string Resolve(string configured)
    {
        var workingDirectory = configured.IsNullOrWhiteSpace()
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(configured);

        if (!Directory.Exists(workingDirectory))
            throw new DirectoryNotFoundException(workingDirectory);

        return workingDirectory;
    }
}
