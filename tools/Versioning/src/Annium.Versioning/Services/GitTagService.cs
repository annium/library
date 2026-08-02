using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using OneOf;

namespace Annium.Versioning.Services;

internal class GitTagService : IGitTagService
{
    public OneOf<IReadOnlyList<string>, string> GetTags(string repositoryPath) =>
        GetTags(repositoryPath, static (_, _) => true);

    public OneOf<IReadOnlyList<string>, string> GetHeadTags(string repositoryPath) =>
        GetTags(repositoryPath, static (tag, headSha) => tag.Target.Sha == headSha);

    public OneOf<IReadOnlyList<string>, string> GetHistoryTags(string repositoryPath) =>
        GetTags(repositoryPath, static (tag, headSha) => tag.Target.Sha != headSha);

    public string? SetTag(string repositoryPath, string tag)
    {
        try
        {
            using var repository = new Repository(repositoryPath);

            var head = repository.Head.Tip;
            if (head is null)
                return $"Failed to set tag {tag} at repo {repositoryPath}: repository has no commits";

            var signature = repository.Config.BuildSignature(DateTimeOffset.Now);
            if (signature is null)
                return $"Failed to set tag {tag} at repo {repositoryPath}: git user.name/user.email are not configured";

            repository.Tags.Add(tag, head, signature, $"Version {tag}");

            return null;
        }
        catch (LibGit2SharpException ex)
        {
            return $"Failed to set tag {tag} at repo {repositoryPath}: {ex}";
        }
    }

    /// <summary>
    /// Opens the repository and projects its tags, filtered against the HEAD commit sha.
    /// The <c>Repository</c> constructor sits inside the try on purpose: it throws
    /// <c>RepositoryNotFoundException</c> for a non-repo path, which must surface as the error
    /// branch of the result rather than escape the service.
    /// </summary>
    private static OneOf<IReadOnlyList<string>, string> GetTags(string repositoryPath, Func<Tag, string?, bool> filter)
    {
        try
        {
            using var repository = new Repository(repositoryPath);

            // null on a repository without commits — no tag target can then match it
            var headSha = repository.Head.Tip?.Sha;

            return repository.Tags.Where(x => filter(x, headSha)).Select(x => x.FriendlyName).ToArray();
        }
        catch (LibGit2SharpException ex)
        {
            return $"Failed to get tags at repo {repositoryPath}: {ex}";
        }
    }
}
