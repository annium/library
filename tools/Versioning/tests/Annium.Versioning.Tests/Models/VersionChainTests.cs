using Annium.Testing;
using Annium.Versioning.Models;
using Xunit;

namespace Annium.Versioning.Tests.Models;

public class VersionChainTests
{
    [Fact]
    public void TryParse_MajorMinor_Parses()
    {
        // act
        var parsed = VersionChain.TryParse("1.2", out var chain);

        // assert
        parsed.IsTrue();
        chain.Major.Is(1u);
        chain.Minor.Is(2u);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1")]
    [InlineData("1.2.3")]
    [InlineData("a.b")]
    [InlineData("1.2-rc1")]
    [InlineData(" 1.2")]
    [InlineData("-1.2")]
    public void TryParse_Malformed_Fails(string? input)
    {
        // act
        var parsed = VersionChain.TryParse(input, out var chain);

        // assert
        parsed.IsFalse();
        chain.Is(default);
    }

    [Fact]
    public void ToString_Chain_RendersMajorMinor()
    {
        // assert
        new VersionChain(3, 14)
            .ToString()
            .Is("3.14");
    }

    [Fact]
    public void Minimal_IsZeroOne()
    {
        // assert
        VersionChain.Minimal.ToString().Is("0.1");
    }
}
