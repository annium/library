using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using OneOf;

namespace Annium.Versioning.Services;

internal class GitTagService : IGitTagService
{
    public OneOf<IReadOnlyList<string>, string> GetTags(string repositoryPath)
    {
        using var repository = new Repository(repositoryPath);

        return repository
            .GetRawTags()
            .Match<OneOf<IReadOnlyList<string>, string>>(
                tags => tags.Select(x => x.FriendlyName).ToArray(),
                error => error
            );
    }

    public OneOf<IReadOnlyList<string>, string> GetHeadTags(string repositoryPath)
    {
        using var repository = new Repository(repositoryPath);
        var headCommit = repository.Head.Tip;

        return repository
            .GetRawTags()
            .Match<OneOf<IReadOnlyList<string>, string>>(
                tags => tags.Where(x => x.Target.Sha == headCommit.Sha).Select(x => x.FriendlyName).ToArray(),
                error => error
            );
    }

    public OneOf<IReadOnlyList<string>, string> GetHistoryTags(string repositoryPath)
    {
        using var repository = new Repository(repositoryPath);
        var headCommit = repository.Head.Tip;

        return repository
            .GetRawTags()
            .Match<OneOf<IReadOnlyList<string>, string>>(
                tags => tags.Where(x => x.Target.Sha != headCommit.Sha).Select(x => x.FriendlyName).ToArray(),
                error => error
            );
    }

    public string? SetTag(string repositoryPath, string tag)
    {
        try
        {
            using var repository = new Repository(repositoryPath);

            var signature = repository.Config.BuildSignature(DateTimeOffset.Now);
            repository.Tags.Add(tag, repository.Head.Tip, signature, $"Version {tag}");

            return null;
        }
        catch (LibGit2SharpException ex)
        {
            return $"Failed to set tag {tag} at repo {repositoryPath}: {ex}";
        }
    }
}

file static class RepositoryExtensions
{
    public static OneOf<IReadOnlyList<Tag>, string> GetRawTags(this Repository repository)
    {
        try
        {
            var tags = repository.Tags.ToArray();

            return tags;
        }
        catch (LibGit2SharpException ex)
        {
            return $"Failed to get head tags at repo {repository.Info.Path}: {ex}";
        }
    }
}
