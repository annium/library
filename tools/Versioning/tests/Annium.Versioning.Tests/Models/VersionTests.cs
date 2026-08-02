using Annium.Testing;
using Annium.Versioning.Models;
using Xunit;
using Version = Annium.Versioning.Models.Version;

namespace Annium.Versioning.Tests.Models;

public class VersionTests
{
    [Fact]
    public void TryParse_Release_ParsesAllParts()
    {
        // act
        var parsed = Version.TryParse("1.2.3", out var version);

        // assert
        parsed.IsTrue();
        version.Major.Is(1u);
        version.Minor.Is(2u);
        version.Patch.Is(3u);
        version.Suffix.Is(string.Empty);
    }

    [Fact]
    public void TryParse_PreRelease_KeepsSuffix()
    {
        // act
        var parsed = Version.TryParse("1.2.3-rc1", out var version);

        // assert
        parsed.IsTrue();
        version.Patch.Is(3u);
        version.Suffix.Is("-rc1");
    }

    [Fact]
    public void TryParse_BuildMetadata_DropsMetadata()
    {
        // act
        var parsed = Version.TryParse("1.2.3+a1b2c3d", out var version);

        // assert
        parsed.IsTrue();
        version.Patch.Is(3u);
        version.Suffix.Is(string.Empty);
    }

    [Fact]
    public void TryParse_PreReleaseWithBuildMetadata_KeepsPreReleaseAndDropsMetadata()
    {
        // act
        var parsed = Version.TryParse("1.2.3-rc1+a1b2c3d", out var version);

        // assert
        parsed.IsTrue();
        version.Patch.Is(3u);
        version.Suffix.Is("-rc1");
    }

    [Theory]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("1.2")]
    [InlineData("x.2.3")]
    [InlineData("1.y.3")]
    [InlineData("1.2.z")]
    [InlineData("1.2.-rc1")]
    [InlineData("-1.2.3")]
    public void TryParse_Malformed_Fails(string raw)
    {
        // act
        var parsed = Version.TryParse(raw, out _);

        // assert
        parsed.IsFalse();
    }

    [Fact]
    public void TryParse_FourSegments_TakesThirdAsPatchAndRestAsSuffix()
    {
        // act
        var parsed = Version.TryParse("1.2.3.4", out var version);

        // assert
        parsed.IsTrue();
        version.Patch.Is(3u);
        version.Suffix.Is(".4");
    }

    [Theory]
    [InlineData("1.2.3")]
    [InlineData("1.2.3-rc1")]
    [InlineData("0.0.0")]
    public void ToString_ParsedVersion_RoundTrips(string raw)
    {
        // arrange
        Version.TryParse(raw, out var version).IsTrue();

        // assert
        version.ToString().Is(raw);
    }

    [Fact]
    public void Empty_Chain_YieldsZeroPatchWithoutSuffix()
    {
        // act
        var version = Version.Empty(new VersionChain(1, 2));

        // assert
        version.ToString().Is("1.2.0");
    }

    [Fact]
    public void CompareTo_ReleaseAgainstOwnPreRelease_RanksReleaseHigher()
    {
        // arrange — regression: string ordering put "" below "-rc1", so `get-version` reported
        // 1.2.3-rc1 as current even though the 1.2.3 release tag existed
        var release = Parse("1.2.3");
        var preRelease = Parse("1.2.3-rc1");

        // assert
        release.IsGreater(preRelease);
        preRelease.IsLess(release);
    }

    [Fact]
    public void CompareTo_NumericPreReleaseIdentifiers_ComparesNumerically()
    {
        // assert — 2 < 10 numerically, but "10" < "2" as text
        Parse("1.0.0-rc.2").IsLess(Parse("1.0.0-rc.10"));
    }

    [Fact]
    public void CompareTo_NumericAgainstAlphanumericIdentifier_RanksNumericLower()
    {
        // assert
        Parse("1.0.0-1").IsLess(Parse("1.0.0-alpha"));
    }

    [Fact]
    public void CompareTo_LongerPreReleaseWithEqualPrefix_RanksHigher()
    {
        // assert
        Parse("1.0.0-alpha").IsLess(Parse("1.0.0-alpha.1"));
    }

    [Fact]
    public void CompareTo_SemVerSpecPrecedenceExample_OrdersAscending()
    {
        // arrange — SemVer 2.0.0 §11 worked example
        var ordered = new[]
        {
            "1.0.0-alpha",
            "1.0.0-alpha.1",
            "1.0.0-alpha.beta",
            "1.0.0-beta",
            "1.0.0-beta.2",
            "1.0.0-beta.11",
            "1.0.0-rc.1",
            "1.0.0",
        };

        // assert
        for (var i = 1; i < ordered.Length; i++)
            Parse(ordered[i - 1]).IsLess(Parse(ordered[i]));
    }

    [Theory]
    [InlineData("1.2.3", "2.0.0")]
    [InlineData("1.2.3", "1.3.0")]
    [InlineData("1.2.3", "1.2.4")]
    public void CompareTo_NumericParts_OrderMajorThenMinorThenPatch(string lower, string higher)
    {
        // assert
        Parse(lower).IsLess(Parse(higher));
    }

    [Fact]
    public void Equals_SameParts_AreEqual()
    {
        // assert
        Parse("1.2.3-rc1").Is(Parse("1.2.3-rc1"));
    }

    private static Version Parse(string raw)
    {
        Version.TryParse(raw, out var version).IsTrue($"failed to parse '{raw}'");

        return version;
    }
}
