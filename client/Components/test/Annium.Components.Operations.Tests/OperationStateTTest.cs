using Annium.Components.State.Operations;
using Annium.Data.Operations;
using Annium.Testing;
using Xunit;

namespace Annium.Components.Operations.Tests;

/// <summary>
/// Tests for generic OperationState&lt;T&gt; operations including start, succeed, fail, and reset scenarios.
/// </summary>
public class OperationStateTTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the OperationStateTTest class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging.</param>
    public OperationStateTTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that starting an operation state correctly sets loading state and fires change notification.
    /// </summary>
    [Fact]
    public void OperationState_Start_Ok()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New<int>);

        // act
        op.Start();

        // assert
        op.Data.Is(0);
        op.HasErrors.IsFalse();
        op.IsLoading.IsTrue();
        op.IsLoaded.IsFalse();
        op.HasSucceed.IsFalse();
        op.HasFailed.IsFalse();
        getChanges().Is(1);
    }

    /// <summary>
    /// Tests that succeeding an operation state with data correctly updates state and fires change notifications.
    /// </summary>
    [Fact]
    public void OperationState_Succeed_Ok()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New<int>);

        // act
        op.Start();
        op.Succeed(5);

        // assert
        op.Data.Is(5);
        op.HasErrors.IsFalse();
        op.IsLoading.IsFalse();
        op.IsLoaded.IsTrue();
        op.HasSucceed.IsTrue();
        op.HasFailed.IsFalse();
        getChanges().Is(2);
    }

    /// <summary>
    /// Tests that failing an operation state with data correctly preserves data and error information.
    /// </summary>
    [Fact]
    public void OperationState_FailedWithData_Ok()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New<int>);
        var failure = Result.New(7).Error("bad").Error("field", "field is empty");

        // act
        op.Start();
        op.Fail(failure);

        // assert
        op.Data.Is(failure.Data);
        op.HasErrors.IsTrue();
        op.PlainErrors.IsEqual(failure.PlainErrors);
        op.LabeledErrors.IsEqual(failure.LabeledErrors);
        op.IsLoading.IsFalse();
        op.IsLoaded.IsTrue();
        op.HasSucceed.IsFalse();
        op.HasFailed.IsTrue();
        getChanges().Is(2);
    }

    /// <summary>
    /// Tests that failing an operation state without data correctly sets error state and preserves default data.
    /// </summary>
    [Fact]
    public void OperationState_Failed_Ok()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New<int>);
        var failure = Result.New().Error("bad").Error("field", "field is empty");

        // act
        op.Start();
        op.Fail(failure);

        // assert
        op.Data.Is(0);
        op.HasErrors.IsTrue();
        op.PlainErrors.IsEqual(failure.PlainErrors);
        op.LabeledErrors.IsEqual(failure.LabeledErrors);
        op.IsLoading.IsFalse();
        op.IsLoaded.IsTrue();
        op.HasSucceed.IsFalse();
        op.HasFailed.IsTrue();
        getChanges().Is(2);
    }

    /// <summary>
    /// Tests that resetting an operation state correctly returns it to initial state and fires change notification.
    /// </summary>
    [Fact]
    public void OperationState_Reset_Ok()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New<int>);

        // act
        op.Start();
        op.Succeed(3);
        op.Reset();

        // assert
        op.Data.Is(0);
        op.HasErrors.IsFalse();
        op.IsLoading.IsFalse();
        op.IsLoaded.IsFalse();
        op.HasSucceed.IsFalse();
        op.HasFailed.IsFalse();
        getChanges().Is(3);
    }
}
