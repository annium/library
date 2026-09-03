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
        var failure = Result.Create(7).Error("bad").Error("field", "field is empty");

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
        var failure = Result.Create().Error("bad").Error("field", "field is empty");

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

    /// <summary>
    /// Tests that Fail via the IResultBase overload (no data) clears a previously-set Data to its default —
    /// proving the data-clearing branch executes (Data is non-default before the Fail).
    /// </summary>
    [Fact]
    public void OperationState_FailAfterSucceed_ClearsData()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New<int>);
        var failure = Result.Create().Error("bad");

        // act: succeed with non-default data, then fail via the IResultBase overload (which discards data)
        op.Start();
        op.Succeed(5);
        op.Data.Is(5);
        op.Fail(failure);

        // assert: Data cleared to default; failure state set
        op.Data.Is(0);
        op.HasFailed.IsTrue();
        op.HasSucceed.IsFalse();
        getChanges().Is(3);
    }

    /// <summary>
    /// Tests that resetting a FAILED operation state clears its populated errors and its data (using the
    /// IDataResultBase overload of Fail, which keeps data, then Reset which must clear both).
    /// </summary>
    [Fact]
    public void OperationState_FailThenReset_ClearsErrorsAndData()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New<int>);
        var failure = Result.Create(7).Error("bad").Error("field", "field is empty");

        // act
        op.Start();
        op.Fail(failure);

        // assert: Fail(IDataResultBase) keeps data and populates errors
        op.Data.Is(7);
        op.HasErrors.IsTrue();

        // act
        op.Reset();

        // assert: Reset clears errors and data
        op.Data.Is(0);
        op.HasErrors.IsFalse();
        op.PlainErrors.Count.Is(0);
        op.LabeledErrors.Count.Is(0);
        op.HasFailed.IsFalse();
        getChanges().Is(3);
    }

    /// <summary>
    /// Tests that re-starting after a successful load clears stale Data — a new load must not render the prior
    /// operation's data as current.
    /// </summary>
    [Fact]
    public void OperationState_StartAfterSucceed_ClearsData()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New<int>);

        // act: succeed with data, then start a new load
        op.Start();
        op.Succeed(5);
        op.Data.Is(5);
        op.Start();

        // assert: the new load cleared stale data
        op.Data.Is(0);
        op.IsLoading.IsTrue();
        op.HasSucceed.IsFalse();
        getChanges().Is(3);
    }

    /// <summary>
    /// Tests that succeeding directly after a failure (no intervening Start/Reset) clears the populated errors
    /// and sets the new data (pins SucceedInternal's error-clearing against a non-empty starting state).
    /// </summary>
    [Fact]
    public void OperationState_SucceedAfterFail_ClearsErrors()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New<int>);

        // act
        op.Start();
        op.Fail(Result.Create().Error("bad").Error("field", "field is empty"));
        op.HasErrors.IsTrue();
        op.Succeed(9);

        // assert: Succeed cleared the prior failure's errors and set the new data
        op.HasErrors.IsFalse();
        op.PlainErrors.Count.Is(0);
        op.LabeledErrors.Count.Is(0);
        op.Data.Is(9);
        op.HasSucceed.IsTrue();
        op.HasFailed.IsFalse();
        getChanges().Is(3);
    }
}
