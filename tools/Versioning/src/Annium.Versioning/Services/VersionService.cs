using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Versioning.Models;

namespace Annium.Versioning.Services;

internal class VersionService : IVersionService, ILogSubject
{
    private readonly IGitTagService _gitTagService;

    public ILogger Logger { get; }

    public VersionService(IGitTagService gitTagService, ILogger logger)
    {
        _gitTagService = gitTagService;
        Logger = logger;
    }

    public Task<Version?> GetCurrentVersionAsync(string repositoryPath)
    {
        var tags = _gitTagService.GetTags(repositoryPath);
        this.Info<int>("Found {TagsCount} tags", tags.Count);

        var versions = tags.Select(tag =>
            {
                if (Version.TryParse(tag, out var version))
                {
                    this.Debug<string, Version>("Parsed tag '{Tag}' as version {Version}", tag, version);
                    return version;
                }
                else
                {
                    this.Debug<string>("Skipping invalid version tag: {Tag}", tag);
                    return null;
                }
            })
            .Where(v => v != null)
            .ToList();

        var result = versions.Count > 0 ? versions.Max() : null;
        return Task.FromResult(result);
    }

    public Task<Version> SetVersionAsync(string repositoryPath, uint major, uint minor)
    {
        var tags = _gitTagService.GetTags(repositoryPath);
        this.Info<int>("Found {TagsCount} tags", tags.Count);

        var versions = tags.Select(tag =>
            {
                if (Version.TryParse(tag, out var version))
                {
                    this.Debug<string, Version>("Parsed tag '{Tag}' as version {Version}", tag, version);
                    return version;
                }
                else
                {
                    this.Debug<string>("Skipping invalid version tag: {Tag}", tag);
                    return null;
                }
            })
            .OfType<Version>()
            .Where(v => v.Major == major && v.Minor == minor)
            .ToList();

        var maxPatch = versions.Count > 0 ? versions.Max(v => v.Patch) : 0u;
        var newVersion = new Version(major, minor, maxPatch + 1, "");

        this.Info<Version>("Creating new version {Version}", newVersion);
        _gitTagService.SetTag(repositoryPath, newVersion.ToString());

        return Task.FromResult(newVersion);
    }
}
