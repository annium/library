using Annium.Testing;
using Xunit;

namespace Annium.Integrations.Social.LinkedIn.Tests;

public class SampleTests : TestBase
{
    public SampleTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Fact]
    public void It_Works()
    {
        true.IsTrue();
    }
}
