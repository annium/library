using System;
using System.Collections.Generic;
using Annium.Logging;
using Annium.Testing;
using Annium.Versioning.Models;
using Annium.Versioning.Services;
using OneOf;
using OneOf.Types;
using Xunit;

namespace Annium.Versioning.Tests.Services;

public class VersionServiceTests
{
    private const string RepositoryPath = "/repo";

    [Fact]
    public void GetCurrentVersion_NoTags_ReturnsRequestedChainAtPatchZero()
    {
        // arrange
        var (service, _) = Setup(tags: []);

        // act
        var result = service.GetCurrentVersion(RepositoryPath, new VersionChain(1, 2));

        // assert
        result.AsT0.ToString().Is("1.2.0");
    }

    [Fact]
    public void GetCurrentVersion_NoTagsAndNoChain_FallsBackToMinimalChain()
    {
        // arrange
        var (service, _) = Setup(tags: []);

        // act
        var result = service.GetCurrentVersion(RepositoryPath);

        // assert
        result.AsT0.ToString().Is("0.1.0");
    }

    [Fact]
    public void GetCurrentVersion_ReleaseAndPreReleaseOnSamePatch_ReturnsRelease()
    {
        // arrange — regression: pre-release used to win the Max() comparison
        var (service, _) = Setup(tags: ["v1.2.3", "v1.2.3-rc1"]);

        // act
        var result = service.GetCurrentVersion(RepositoryPath, new VersionChain(1, 2));

        // assert
        result.AsT0.ToString().Is("1.2.3");
    }

    [Fact]
    public void GetCurrentVersion_TagsAcrossChains_ReturnsHighestOfRequestedChain()
    {
        // arrange
        var (service, _) = Setup(tags: ["v1.2.1", "v1.2.7", "v1.3.9", "v2.0.0"]);

        // act
        var result = service.GetCurrentVersion(RepositoryPath, new VersionChain(1, 2));

        // assert
        result.AsT0.ToString().Is("1.2.7");
    }

    [Fact]
    public void GetCurrentVersion_NoChain_ReturnsHighestAcrossAllChains()
    {
        // arrange
        var (service, _) = Setup(tags: ["v1.2.1", "v1.3.9", "v2.0.0"]);

        // act
        var result = service.GetCurrentVersion(RepositoryPath);

        // assert
        result.AsT0.ToString().Is("2.0.0");
    }

    [Theory]
    [InlineData("1.2.3")] // missing the `v` prefix
    [InlineData("release-1.2.3")]
    [InlineData("v1.2")]
    [InlineData("vx.y.z")]
    // every other case fails to parse even once its first character is dropped, so none of them tells
    // the `v` check apart from no check at all
    [InlineData("x1.2.9")]
    public void GetCurrentVersion_NonVersionTags_AreIgnored(string tag)
    {
        // arrange
        var (service, _) = Setup(tags: [tag]);

        // act
        var result = service.GetCurrentVersion(RepositoryPath, new VersionChain(1, 2));

        // assert
        result.AsT0.ToString().Is("1.2.0");
    }

    [Fact]
    public void GetCurrentVersion_GitFailure_PropagatesError()
    {
        // arrange
        var (service, _) = Setup(tagsError: "boom");

        // act
        var result = service.GetCurrentVersion(RepositoryPath, new VersionChain(1, 2));

        // assert
        result.IsT1.IsTrue();
        result.AsT1.Is("boom");
    }

    [Fact]
    public void SetVersion_HeadAlreadyTaggedInChain_ReturnsExistingWithoutTagging()
    {
        // arrange
        var (service, git) = Setup(headTags: ["v1.2.5"]);

        // act
        var result = service.SetVersion(RepositoryPath, new VersionChain(1, 2));

        // assert
        result.AsT0.ToString().Is("1.2.5");
        git.SetTags.IsEmpty();
    }

    [Fact]
    public void SetVersion_HeadTaggedInOtherChainOnly_CreatesNewVersion()
    {
        // arrange
        var (service, git) = Setup(headTags: ["v9.9.9"], historyTags: []);

        // act
        var result = service.SetVersion(RepositoryPath, new VersionChain(1, 2));

        // assert
        result.AsT0.ToString().Is("1.2.0");
        git.SetTags.Has(1).At(0).Is("v1.2.0");
    }

    [Fact]
    public void SetVersion_NoHistoryInChain_CreatesPatchZero()
    {
        // arrange
        var (service, git) = Setup(historyTags: ["v3.4.8"]);

        // act
        var result = service.SetVersion(RepositoryPath, new VersionChain(1, 2));

        // assert
        result.AsT0.ToString().Is("1.2.0");
        git.SetTags.Has(1).At(0).Is("v1.2.0");
    }

    [Fact]
    public void SetVersion_HistoryInChain_IncrementsHighestPatch()
    {
        // arrange
        var (service, git) = Setup(historyTags: ["v1.2.0", "v1.2.7", "v1.2.3", "v1.3.0"]);

        // act
        var result = service.SetVersion(RepositoryPath, new VersionChain(1, 2));

        // assert
        result.AsT0.ToString().Is("1.2.8");
        git.SetTags.Has(1).At(0).Is("v1.2.8");
    }

    [Fact]
    public void SetVersion_HeadTagsFailure_PropagatesError()
    {
        // arrange
        var (service, git) = Setup(headTagsError: "head boom");

        // act
        var result = service.SetVersion(RepositoryPath, new VersionChain(1, 2));

        // assert
        result.AsT1.Is("head boom");
        git.SetTags.IsEmpty();
    }

    [Fact]
    public void SetVersion_HistoryTagsFailure_PropagatesError()
    {
        // arrange
        var (service, git) = Setup(historyTagsError: "history boom");

        // act
        var result = service.SetVersion(RepositoryPath, new VersionChain(1, 2));

        // assert
        result.AsT1.Is("history boom");
        git.SetTags.IsEmpty();
    }

    [Fact]
    public void SetVersion_TaggingFailure_PropagatesError()
    {
        // arrange
        var (service, _) = Setup(setTagError: "tag boom");

        // act
        var result = service.SetVersion(RepositoryPath, new VersionChain(1, 2));

        // assert
        result.IsT1.IsTrue();
        result.AsT1.Is("tag boom");
    }

    private static (VersionService Service, TestGitTagService Git) Setup(
        IReadOnlyList<string>? tags = null,
        IReadOnlyList<string>? headTags = null,
        IReadOnlyList<string>? historyTags = null,
        string? tagsError = null,
        string? headTagsError = null,
        string? historyTagsError = null,
        string? setTagError = null
    )
    {
        var git = new TestGitTagService
        {
            Tags = tagsError is null ? Result(tags) : tagsError,
            HeadTags = headTagsError is null ? Result(headTags) : headTagsError,
            HistoryTags = historyTagsError is null ? Result(historyTags) : historyTagsError,
            SetTagError = setTagError,
        };

        return (new VersionService(git, VoidLogger.Instance), git);
    }

    private static OneOf<IReadOnlyList<string>, string> Result(IReadOnlyList<string>? tags) =>
        OneOf<IReadOnlyList<string>, string>.FromT0(tags ?? Array.Empty<string>());

    private sealed class TestGitTagService : IGitTagService
    {
        public OneOf<IReadOnlyList<string>, string> Tags { get; init; } = Array.Empty<string>();
        public OneOf<IReadOnlyList<string>, string> HeadTags { get; init; } = Array.Empty<string>();
        public OneOf<IReadOnlyList<string>, string> HistoryTags { get; init; } = Array.Empty<string>();
        public string? SetTagError { get; init; }

        public List<string> SetTags { get; } = new();

        public OneOf<IReadOnlyList<string>, string> GetTags(string repositoryPath) => Tags;

        public OneOf<IReadOnlyList<string>, string> GetHeadTags(string repositoryPath) => HeadTags;

        public OneOf<IReadOnlyList<string>, string> GetHistoryTags(string repositoryPath) => HistoryTags;

        public OneOf<Success, string> SetTag(string repositoryPath, string tag)
        {
            if (SetTagError is not null)
                return SetTagError;

            SetTags.Add(tag);

            return new Success();
        }
    }
}
