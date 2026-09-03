using System;
using System.IO;
using Annium.Testing;
using Annium.Versioning.Commands;
using Xunit;

namespace Annium.Versioning.Tests.Commands;

// Both commands resolve their repository path through this helper, so a dropped existence check would
// have get-version and set-version derive a version from a path that is not there.
public class WorkingDirectoryTests
{
    [Fact]
    public void Resolve_MissingDirectory_Throws()
    {
        // arrange
        var missing = Path.Combine(Path.GetTempPath(), $"versioning-missing-{Guid.NewGuid():N}");

        // act
        var exception = Wrap.It(() => WorkingDirectory.Resolve(missing)).Throws<DirectoryNotFoundException>();

        // assert
        exception.Message.IsEqual(missing);
    }

    [Fact]
    public void Resolve_ExistingRelativeDirectory_ReturnsItsFullPath()
    {
        // arrange
        var relative = Path.GetRelativePath(Directory.GetCurrentDirectory(), Path.GetTempPath());

        // act
        var resolved = WorkingDirectory.Resolve(relative);

        // assert
        resolved.IsEqual(Path.GetFullPath(relative));
    }

    [Fact]
    public void Resolve_NoDirectory_FallsBackToTheCurrentOne()
    {
        // act
        var resolved = WorkingDirectory.Resolve(string.Empty);

        // assert
        resolved.IsEqual(Directory.GetCurrentDirectory());
    }
}
