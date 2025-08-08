using System.Collections.Generic;
using System.Linq;
using Annium.Logging;
using Annium.Versioning.Models;
using OneOf;

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

    public OneOf<Version, string> GetCurrentVersion(string repositoryPath, VersionChain? versionChain = null)
    {
        this.Trace<string>(
            "finding current version for chain {VersionChain} on HEAD",
            versionChain?.ToString() ?? "all"
        );
        var tagsResult = _gitTagService.GetTags(repositoryPath);
        if (tagsResult.IsT1)
            return tagsResult.AsT1;

        var tags = tagsResult.AsT0;
        var versions = ParseVersions(tags);
        var filteredVersions = versionChain is null ? versions : FilterByVersionChain(versions, versionChain.Value);

        this.Trace<int, int, int, string>(
            "found {TagsCount} tags as {VersionsCount} versions, filtered to {FilteredCount} versions for chain {Chain}",
            tags.Count,
            versions.Count,
            filteredVersions.Count,
            versionChain?.ToString() ?? "all"
        );

        var existing = filteredVersions.Count > 0 ? filteredVersions.Max() : null;
        var result = existing ?? Version.Empty(versionChain ?? VersionChain.Minimal);

        return result;
    }

    public OneOf<Version, string> SetVersion(string repositoryPath, VersionChain versionChain)
    {
        this.Trace("setting version for chain {VersionChain} on HEAD", versionChain);

        // get HEAD commit tags
        var headTagsResult = _gitTagService.GetHeadTags(repositoryPath);
        if (headTagsResult.IsT1)
            return headTagsResult.AsT1;

        var headTags = headTagsResult.AsT0;
        var headVersions = ParseVersions(headTags);
        var headFilteredVersions = FilterByVersionChain(headVersions, versionChain);
        this.Trace(
            "found {TagsCount} tags as {VersionsCount} versions, filtered to {FilteredCount} versions for chain {VersionChain} on HEAD",
            headTags.Count,
            headVersions.Count,
            headFilteredVersions.Count,
            versionChain
        );

        if (headFilteredVersions.Count > 0)
        {
            var version = headFilteredVersions.Max().NotNull();
            this.Trace("version with chain {VersionChain} already exists on HEAD: {Version}", versionChain, version);
            return version;
        }

        // get history tags (excluding HEAD commit tags)
        var tagsResult = _gitTagService.GetHistoryTags(repositoryPath);
        if (tagsResult.IsT1)
            return tagsResult.AsT1;

        var tags = tagsResult.AsT0;
        var versions = ParseVersions(tags);
        var filteredVersions = FilterByVersionChain(versions, versionChain);
        this.Trace(
            "found {TagsCount} tags as {VersionsCount} versions, filtered to {FilteredCount} versions for chain {VersionChain}",
            tags.Count,
            versions.Count,
            filteredVersions.Count,
            versionChain
        );

        var patch = filteredVersions.Count > 0 ? filteredVersions.Max(v => v.Patch) + 1 : 0u;
        var newVersion = new Version(versionChain.Major, versionChain.Minor, patch, "");

        this.Trace("creating new version {Version}", newVersion);
        var tag = $"v{newVersion}";
        var setTagError = _gitTagService.SetTag(repositoryPath, tag);

        return setTagError is null ? newVersion : setTagError;
    }

    private IReadOnlyList<Version> ParseVersions(IReadOnlyList<string> tags)
    {
        return tags.Select(tag =>
            {
                if (tag.StartsWith('v') && Version.TryParse(tag[1..], out var version))
                {
                    this.Trace("parsed tag '{Tag}' as version {Version}", tag, version);
                    return version;
                }

                this.Trace<string>("skipping invalid version tag: {Tag}", tag);
                return null;
            })
            .OfType<Version>()
            .ToArray();
    }

    private IReadOnlyList<Version> FilterByVersionChain(IReadOnlyList<Version> versions, VersionChain chain)
    {
        return versions.Where(v => v.Major == chain.Major && v.Minor == chain.Minor).ToArray();
    }
}
