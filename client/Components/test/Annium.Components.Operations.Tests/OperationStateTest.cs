using Annium.Components.State.Operations;
using Annium.Data.Operations;
using Annium.Testing;
using Xunit;

namespace Annium.Components.Operations.Tests;

public class OperationStateTest : TestBase
{
    public OperationStateTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Fact]
    public void OperationState_Start_Ok()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New);

        // act
        op.Start();

        // assert
        op.HasErrors.IsFalse();
        op.IsLoading.IsTrue();
        op.IsLoaded.IsFalse();
        op.HasSucceed.IsFalse();
        op.HasFailed.IsFalse();
        getChanges().Is(1);
    }

    [Fact]
    public void OperationState_Succeed_Ok()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New);

        // act
        op.Start();
        op.Succeed();

        // assert
        op.HasErrors.IsFalse();
        op.IsLoading.IsFalse();
        op.IsLoaded.IsTrue();
        op.HasSucceed.IsTrue();
        op.HasFailed.IsFalse();
        getChanges().Is(2);
    }

    [Fact]
    public void OperationState_Failed_Ok()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New);
        var failure = Result.New().Error("bad").Error("field", "field is empty");

        // act
        op.Start();
        op.Fail(failure);

        // assert
        op.HasErrors.IsTrue();
        op.PlainErrors.IsEqual(failure.PlainErrors);
        op.LabeledErrors.IsEqual(failure.LabeledErrors);
        op.IsLoading.IsFalse();
        op.IsLoaded.IsTrue();
        op.HasSucceed.IsFalse();
        op.HasFailed.IsTrue();
        getChanges().Is(2);
    }

    [Fact]
    public void OperationState_Reset_Ok()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New);

        // act
        op.Start();
        op.Succeed();
        op.Reset();

        // assert
        op.HasErrors.IsFalse();
        op.IsLoading.IsFalse();
        op.IsLoaded.IsFalse();
        op.HasSucceed.IsFalse();
        op.HasFailed.IsFalse();
        getChanges().Is(3);
    }
}
