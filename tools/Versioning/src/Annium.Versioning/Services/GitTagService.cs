using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace Annium.Versioning.Services;

internal class GitTagService : IGitTagService
{
    public IReadOnlyList<string> GetTags(string repositoryPath)
    {
        using var repository = new Repository(repositoryPath);
        return repository.Tags.Select(tag => tag.FriendlyName).ToArray();
    }

    public void SetTag(string repositoryPath, string tag)
    {
        using var repository = new Repository(repositoryPath);
        var signature = repository.Config.BuildSignature(DateTimeOffset.Now);
        repository.Tags.Add(tag, repository.Head.Tip, signature, $"Version {tag}");
    }
}
