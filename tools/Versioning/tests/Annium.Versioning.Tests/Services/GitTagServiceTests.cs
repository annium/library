using System;
using System.IO;
using System.Linq;
using Annium.Testing;
using Annium.Versioning.Services;
using LibGit2Sharp;
using Xunit;

namespace Annium.Versioning.Tests.Services;

// The rest of the suite drives VersionService through a fake IGitTagService, so the LibGit2Sharp
// layer is only covered here — against real throwaway repositories, since what it has to get right
// (which tags HEAD can reach) has no meaning without real history.
public class GitTagServiceTests : IDisposable
{
    private readonly GitTagService _service = new();
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"versioning-tests-{Guid.NewGuid():N}");

    [Fact]
    public void GetTags_TagOnAnUnreachableBranch_IsExcluded()
    {
        // arrange — v1.2.99 lives on an abandoned branch; a version derived from it would outrank
        // everything the checked-out history actually contains
        using var repository = InitRepository();
        var first = Commit(repository, "one");
        Tag(repository, "v1.2.1", first);
        var feature = repository.CreateBranch("feature", first);
        LibGit2Sharp.Commands.Checkout(repository, feature);
        Tag(repository, "v1.2.99", Commit(repository, "on the branch"));
        LibGit2Sharp.Commands.Checkout(repository, repository.Branches.Single(x => x.FriendlyName != "feature"));
        Tag(repository, "v1.2.7", Commit(repository, "two"));

        // act
        var tags = _service.GetTags(_path);

        // assert
        var reachable = tags.AsT0.OrderBy(x => x).ToArray();
        reachable.Has(2);
        reachable.At(0).Is("v1.2.1");
        reachable.At(1).Is("v1.2.7");
    }

    [Fact]
    public void GetHeadTags_ReturnsOnlyTagsOnTheCheckedOutCommit()
    {
        // arrange
        using var repository = InitRepository();
        Tag(repository, "v1.2.1", Commit(repository, "one"));
        Tag(repository, "v1.2.7", Commit(repository, "two"));

        // act
        var tags = _service.GetHeadTags(_path);

        // assert
        tags.AsT0.Has(1).At(0).Is("v1.2.7");
    }

    [Fact]
    public void GetHistoryTags_ReturnsReachableTagsExceptTheHeadOnes()
    {
        // arrange
        using var repository = InitRepository();
        Tag(repository, "v1.2.1", Commit(repository, "one"));
        Tag(repository, "v1.2.7", Commit(repository, "two"));

        // act
        var tags = _service.GetHistoryTags(_path);

        // assert
        tags.AsT0.Has(1).At(0).Is("v1.2.1");
    }

    [Fact]
    public void GetTags_AnnotatedTagAndTagOnThatTag_AreBothCounted()
    {
        // arrange — an annotated tag targets its own annotation object, and a tag on a tag chains
        // further, so a reachability check against raw targets drops both
        using var repository = InitRepository();
        var commit = Commit(repository, "one");
        var signature = new Signature("test", "test@annium.com", DateTimeOffset.UnixEpoch);
        var annotated = repository.Tags.Add("v1.2.1", commit, signature, "release");
        repository.Tags.Add("v1.2.2", annotated.Annotation, signature, "release of a release");

        // act
        var tags = _service.GetTags(_path);

        // assert
        var found = tags.AsT0.OrderBy(x => x).ToArray();
        found.Has(2);
        found.At(0).Is("v1.2.1");
        found.At(1).Is("v1.2.2");
    }

    [Fact]
    public void GetTags_RepositoryWithoutCommits_ReportsNoTags()
    {
        // arrange — HEAD reaches nothing, and the empty-repository path used to be an NRE
        using var repository = InitRepository();

        // act
        var tags = _service.GetTags(_path);

        // assert
        tags.AsT0.IsEmpty();
    }

    [Fact]
    public void GetTags_PathIsNotARepository_ReturnsTheError()
    {
        // arrange
        Directory.CreateDirectory(_path);

        // act
        var tags = _service.GetTags(_path);

        // assert
        tags.IsT1.IsTrue();
        tags.AsT1.IsContaining("Failed to get tags");
    }

    [Fact]
    public void SetTag_TagsTheCheckedOutCommit()
    {
        // arrange — SetTag signs the tag with the repository's configured identity, which is written
        // locally here so the test does not depend on the machine's global git config (a CI runner
        // has none)
        using var repository = InitRepository();
        Commit(repository, "one");

        // act
        var result = _service.SetTag(_path, "v1.2.0");

        // assert
        result.IsT0.IsTrue();
        repository.Tags.Has(1).At(0).PeeledTarget.Sha.Is(repository.Head.Tip.Sha);
    }

    [Fact]
    public void SetTag_RepositoryWithoutCommits_ReturnsTheError()
    {
        // arrange — there is nothing to tag; the guard exists because reading Head.Tip.Sha here used
        // to throw straight past the result contract
        using var repository = InitRepository();

        // act
        var result = _service.SetTag(_path, "v1.2.0");

        // assert
        result.IsT1.IsTrue();
        result.AsT1.IsContaining("repository has no commits");
        repository.Tags.IsEmpty();
    }

    public void Dispose()
    {
        if (!Directory.Exists(_path))
            return;

        // libgit2 leaves its object files read-only, which Directory.Delete refuses
        foreach (var file in Directory.EnumerateFiles(_path, "*", SearchOption.AllDirectories))
            File.SetAttributes(file, FileAttributes.Normal);

        Directory.Delete(_path, recursive: true);
    }

    private Repository InitRepository()
    {
        Repository.Init(_path);

        var repository = new Repository(_path);
        // written locally so nothing here reads the machine's global git config
        repository.Config.Set("user.name", "test", ConfigurationLevel.Local);
        repository.Config.Set("user.email", "test@annium.com", ConfigurationLevel.Local);

        return repository;
    }

    private Commit Commit(Repository repository, string message)
    {
        // staged by its repository-relative name: the temp root resolves through a symlink on macOS
        // (/var -> /private/var), which libgit2 rejects as outside the working directory
        var file = $"{Guid.NewGuid():N}.txt";
        File.WriteAllText(Path.Combine(_path, file), message);
        LibGit2Sharp.Commands.Stage(repository, file);

        var signature = new Signature("test", "test@annium.com", DateTimeOffset.UnixEpoch);

        return repository.Commit(message, signature, signature);
    }

    private static void Tag(Repository repository, string name, Commit commit) => repository.Tags.Add(name, commit);
}
