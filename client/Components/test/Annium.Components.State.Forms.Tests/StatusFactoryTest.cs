using Annium.Testing;
using Xunit;

namespace Annium.Components.State.Forms.Tests;

/// <summary>
/// Tests for the StatusFactory helper methods that create StateStatus instances
/// </summary>
public class StatusFactoryTest
{
    /// <summary>
    /// Tests that Default has None status and an empty message
    /// </summary>
    [Fact]
    public void Default_Ok()
    {
        // act
        var status = StatusFactory.Default;

        // assert
        status.Value.Is(Status.None);
        status.Message.Is(string.Empty);
    }

    /// <summary>
    /// Tests that None creates a StateStatus with None status, defaulting to an empty message
    /// </summary>
    [Fact]
    public void None_Ok()
    {
        // act
        var status = StatusFactory.None();

        // assert
        status.Value.Is(Status.None);
        status.Message.Is(string.Empty);

        // act
        var withMessage = StatusFactory.None("info");

        // assert
        withMessage.Value.Is(Status.None);
        withMessage.Message.Is("info");
    }

    /// <summary>
    /// Tests that Loading creates a StateStatus with Loading status and the specified message
    /// </summary>
    [Fact]
    public void Loading_Ok()
    {
        // act
        var status = StatusFactory.Loading("please wait");

        // assert
        status.Value.Is(Status.Loading);
        status.Message.Is("please wait");
    }

    /// <summary>
    /// Tests that Validating creates a StateStatus with Validating status and the specified message
    /// </summary>
    [Fact]
    public void Validating_Ok()
    {
        // act
        var status = StatusFactory.Validating("checking");

        // assert
        status.Value.Is(Status.Validating);
        status.Message.Is("checking");
    }

    /// <summary>
    /// Tests that Success creates a StateStatus with Success status and the specified message
    /// </summary>
    [Fact]
    public void Success_Ok()
    {
        // act
        var status = StatusFactory.Success("done");

        // assert
        status.Value.Is(Status.Success);
        status.Message.Is("done");
    }

    /// <summary>
    /// Tests that Error creates a StateStatus with Error status and the specified message
    /// </summary>
    [Fact]
    public void Error_Ok()
    {
        // act
        var status = StatusFactory.Error("boom");

        // assert
        status.Value.Is(Status.Error);
        status.Message.Is("boom");
    }
}
