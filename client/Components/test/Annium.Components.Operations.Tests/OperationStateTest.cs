using System.Collections.Generic;
using System.Linq;
using Annium.Components.State.Operations;
using Annium.Data.Operations;
using Annium.Testing;
using Xunit;

namespace Annium.Components.Operations.Tests;

/// <summary>
/// Tests for non-generic OperationState operations including start, succeed, fail, and reset scenarios.
/// </summary>
public class OperationStateTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the OperationStateTest class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging.</param>
    public OperationStateTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that starting an operation state correctly sets loading state and fires change notification.
    /// </summary>
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

    /// <summary>
    /// Tests that succeeding an operation state correctly updates state and fires change notifications.
    /// </summary>
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

    /// <summary>
    /// Tests that failing an operation state correctly sets error state and fires change notifications.
    /// </summary>
    [Fact]
    public void OperationState_Failed_Ok()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New);
        var failure = Result.Create().Error("bad").Error("field", "field is empty");

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

    /// <summary>
    /// Tests that resetting an operation state correctly returns it to initial state and fires change notification.
    /// </summary>
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

    /// <summary>
    /// Tests that resetting a FAILED operation state clears its populated plain and labeled errors (pins the
    /// error-clearing branch of Reset against a non-empty starting state).
    /// </summary>
    [Fact]
    public void OperationState_FailThenReset_ClearsErrors()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New);
        var failure = Result.Create().Error("bad").Error("field", "field is empty");

        // act
        op.Start();
        op.Fail(failure);

        // assert: errors populated after Fail
        op.HasErrors.IsTrue();
        op.PlainErrors.Count.Is(1);
        op.LabeledErrors.Count.Is(1);

        // act
        op.Reset();

        // assert: Reset clears the populated errors and flags
        op.HasErrors.IsFalse();
        op.PlainErrors.Count.Is(0);
        op.LabeledErrors.Count.Is(0);
        op.IsLoading.IsFalse();
        op.IsLoaded.IsFalse();
        op.HasFailed.IsFalse();
        getChanges().Is(3);
    }

    /// <summary>
    /// Tests that PlainError joins the plain errors with a semicolon separator, and is empty when there are none.
    /// </summary>
    [Fact]
    public void OperationState_PlainError_JoinsPlainErrors()
    {
        // arrange
        var (op, _) = Arrange(OperationState.New);

        // assert: no errors → empty joined string
        op.PlainError.Is(string.Empty);

        // act
        op.Start();
        op.Fail(Result.Create().Error("bad1").Error("bad2"));

        // assert: plain errors joined with "; "
        op.PlainError.Is("bad1; bad2");
    }

    /// <summary>
    /// Tests that IsOk reflects the error state across the operation lifecycle (true with no errors, false after
    /// a failure, true again after reset).
    /// </summary>
    [Fact]
    public void OperationState_IsOk_ReflectsErrorState()
    {
        // arrange
        var (op, _) = Arrange(OperationState.New);

        // assert: ok initially and while loading
        op.IsOk.IsTrue();
        op.Start();
        op.IsOk.IsTrue();

        // act + assert: not ok after a failure
        op.Fail(Result.Create().Error("bad"));
        op.IsOk.IsFalse();

        // act + assert: ok again after reset
        op.Reset();
        op.IsOk.IsTrue();
    }

    /// <summary>
    /// Tests that ErrorState() formats both the no-error case and the populated plain + labeled error case.
    /// </summary>
    [Fact]
    public void OperationState_ErrorState_FormatsErrors()
    {
        // arrange
        var (op, _) = Arrange(OperationState.New);

        // assert: no-error formatting
        var empty = op.ErrorState();
        empty.Contains("no plain errors").IsTrue();
        empty.Contains("no labeled errors").IsTrue();

        // act
        op.Start();
        op.Fail(Result.Create().Error("bad").Error("field", "field is empty"));

        // assert: populated-error formatting includes counts and each error's text
        var state = op.ErrorState();
        state.Contains("1 plain errors").IsTrue();
        state.Contains("- bad").IsTrue();
        state.Contains("1 labeled errors").IsTrue();
        state.Contains("- field:").IsTrue();
        state.Contains("-- field is empty").IsTrue();
    }

    /// <summary>
    /// Tests that ErrorState() takes the ASYMMETRIC branches: a plain-only failure prints the plain section AND
    /// the "no labeled errors" fallback; a labeled-only failure prints the labeled section AND the "no plain
    /// errors" fallback. Pins each of the two independent if/else gates against a count-field swap that the
    /// both-empty / both-populated cases cannot distinguish (their counts move together).
    /// </summary>
    [Fact]
    public void OperationState_ErrorState_FormatsAsymmetricErrors()
    {
        // arrange: plain-only failure with TWO plain errors (also pins ErrorState's per-item plain loop, the
        // analogue of the multi-label / multi-message labeled loops)
        var (plainOp, _) = Arrange(OperationState.New);
        plainOp.Start();
        plainOp.Fail(Result.Create().Error("bad1").Error("bad2"));

        // assert: plain section lists EACH error, labeled fallback taken
        var plainState = plainOp.ErrorState();
        plainState.Contains("2 plain errors").IsTrue();
        plainState.Contains("- bad1").IsTrue();
        plainState.Contains("- bad2").IsTrue();
        plainState.Contains("no labeled errors").IsTrue();
        plainState.Contains("no plain errors").IsFalse();

        // arrange: labeled-only failure
        var (labeledOp, _) = Arrange(OperationState.New);
        labeledOp.Start();
        labeledOp.Fail(Result.Create().Error("field", "field is empty"));

        // assert: labeled section present, plain fallback taken
        var labeledState = labeledOp.ErrorState();
        labeledState.Contains("1 labeled errors").IsTrue();
        labeledState.Contains("-- field is empty").IsTrue();
        labeledState.Contains("no plain errors").IsTrue();
        labeledState.Contains("no labeled errors").IsFalse();
    }

    /// <summary>
    /// Tests that a failure with ONLY labeled errors (no plain errors) still reports HasErrors/!IsOk (pins the
    /// labeled-only branch of HasErrors, which the combined-error tests can't isolate).
    /// </summary>
    [Fact]
    public void OperationState_LabeledOnlyError_HasErrors()
    {
        // arrange
        var (op, _) = Arrange(OperationState.New);

        // act
        op.Start();
        op.Fail(Result.Create().Error("field", "field is empty"));

        // assert
        op.HasErrors.IsTrue();
        op.IsOk.IsFalse();
        op.PlainErrors.Count.Is(0);
        op.LabeledErrors.Count.Is(1);
    }

    /// <summary>
    /// Tests that a failure with ONLY plain errors (no labeled errors) still reports HasErrors/!IsOk (pins the
    /// plain-only operand of HasErrors — the mirror of the labeled-only case, which the combined-error tests
    /// can't isolate because the labeled operand masks a mutation of the plain comparison).
    /// </summary>
    [Fact]
    public void OperationState_PlainOnlyError_HasErrors()
    {
        // arrange
        var (op, _) = Arrange(OperationState.New);

        // act
        op.Start();
        op.Fail(Result.Create().Error("bad"));

        // assert
        op.HasErrors.IsTrue();
        op.IsOk.IsFalse();
        op.PlainErrors.Count.Is(1);
        op.LabeledErrors.Count.Is(0);
    }

    /// <summary>
    /// Tests that ErrorState() lists every message under a label (pins the inner per-label message loop, which
    /// the single-message case cannot).
    /// </summary>
    [Fact]
    public void OperationState_ErrorState_ListsMultipleMessagesPerLabel()
    {
        // arrange
        var (op, _) = Arrange(OperationState.New);

        // act
        op.Start();
        op.Fail(Result.Create().Error("field", "first").Error("field", "second"));

        // assert: both messages under the same label are printed
        var state = op.ErrorState();
        state.Contains("-- first").IsTrue();
        state.Contains("-- second").IsTrue();
    }

    /// <summary>
    /// Tests that re-starting after a FAILED run clears the populated errors and failed flag (Start() must clear
    /// prior error state, distinct from Reset()).
    /// </summary>
    [Fact]
    public void OperationState_StartAfterFail_ClearsErrors()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New);

        // act
        op.Start();
        op.Fail(Result.Create().Error("bad").Error("field", "field is empty"));
        op.HasErrors.IsTrue();
        op.Start();

        // assert: a new load clears the prior failure's errors and flag
        op.HasErrors.IsFalse();
        op.PlainErrors.Count.Is(0);
        op.LabeledErrors.Count.Is(0);
        op.HasFailed.IsFalse();
        op.IsLoading.IsTrue();
        getChanges().Is(3);
    }

    /// <summary>
    /// Tests that succeeding directly after a failure (no intervening Start/Reset) clears the populated errors
    /// (pins SucceedInternal's own error-clearing against a non-empty starting state).
    /// </summary>
    [Fact]
    public void OperationState_SucceedAfterFail_ClearsErrors()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New);

        // act
        op.Start();
        op.Fail(Result.Create().Error("bad").Error("field", "field is empty"));
        op.HasErrors.IsTrue();
        op.Succeed();

        // assert: Succeed cleared the prior failure's errors
        op.HasErrors.IsFalse();
        op.IsOk.IsTrue();
        op.PlainErrors.Count.Is(0);
        op.LabeledErrors.Count.Is(0);
        op.HasSucceed.IsTrue();
        op.HasFailed.IsFalse();
        getChanges().Is(3);
    }

    /// <summary>
    /// Tests that ErrorState() lists EVERY distinct label (pins the outer per-label loop, which the
    /// single-label / multi-message-per-label cases cannot).
    /// </summary>
    [Fact]
    public void OperationState_ErrorState_ListsAllLabels()
    {
        // arrange
        var (op, _) = Arrange(OperationState.New);

        // act
        op.Start();
        op.Fail(Result.Create().Error("email", "invalid").Error("password", "too short"));

        // assert: both distinct labels and their messages are printed
        var state = op.ErrorState();
        state.Contains("2 labeled errors").IsTrue();
        state.Contains("- email:").IsTrue();
        state.Contains("-- invalid").IsTrue();
        state.Contains("- password:").IsTrue();
        state.Contains("-- too short").IsTrue();
    }

    /// <summary>
    /// Tests that transitions performed inside a Mute() scope update state but suppress the Changed notification,
    /// and disposing the scope does not itself re-fire (Mute is inherited from ObservableState).
    /// </summary>
    [Fact]
    public void OperationState_MutedTransitions_SuppressNotifications()
    {
        // arrange
        var (op, getChanges) = Arrange(OperationState.New);

        // act: transitions inside a Mute scope
        using (op.Mute())
        {
            op.Start();
            op.Succeed();
        }

        // assert: state reflects the last transition, but no notification fired (nor on scope disposal)
        op.HasSucceed.IsTrue();
        op.IsLoading.IsFalse();
        getChanges().Is(0);
    }

    /// <summary>
    /// Tests that Fail defensively snapshots the result's error collections: mutating the original (live)
    /// collections after the failure was recorded does not change the operation state's exposed errors.
    /// </summary>
    [Fact]
    public void OperationState_Fail_SnapshotsErrorCollections()
    {
        // arrange: a result whose error collections are LIVE (unlike the real Result, which snapshots)
        var (op, _) = Arrange(OperationState.New);
        var result = new MutableResult();
        result.Plain.Add("bad");
        var fieldErrors = new List<string> { "x" };
        result.Labeled["field"] = fieldErrors;

        // act
        op.Start();
        op.Fail(result);

        // mutate the source's live collections AFTER the failure was recorded: the outer list/dictionary AND an
        // existing label's inner message list (pins both the outer copy and the per-label x.Value.ToArray() copy)
        result.Plain.Add("injected");
        result.Labeled["other"] = new List<string> { "injected" };
        fieldErrors.Add("injected");

        // assert: the operation state snapshotted the collections at Fail time (defensive copy, inner + outer)
        op.PlainErrors.Count.Is(1);
        op.LabeledErrors.Count.Is(1);
        op.LabeledErrors["field"].Count.Is(1);
        op.LabeledErrors["field"].Contains("injected").IsFalse();
    }

    /// <summary>
    /// An <see cref="IResultBase"/> whose error collections are live mutable backing collections (unlike the
    /// real Annium Result, which returns fresh snapshots) — used to prove OperationState copies them defensively.
    /// </summary>
    private sealed class MutableResult : IResultBase
    {
        /// <summary>
        /// Gets the live plain-error backing list.
        /// </summary>
        public List<string> Plain { get; } = new();

        /// <summary>
        /// Gets the live labeled-error backing dictionary.
        /// </summary>
        public Dictionary<string, IReadOnlyCollection<string>> Labeled { get; } = new();

        /// <summary>
        /// Gets the live plain-error collection.
        /// </summary>
        public IReadOnlyCollection<string> PlainErrors => Plain;

        /// <summary>
        /// Gets the plain errors joined by a separator.
        /// </summary>
        public string PlainError => string.Join("; ", Plain);

        /// <summary>
        /// Gets the live labeled-error collection.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyCollection<string>> LabeledErrors => Labeled;

        /// <summary>
        /// Gets a value indicating whether there are no errors.
        /// </summary>
        public bool IsOk => Plain.Count == 0 && Labeled.Count == 0;

        /// <summary>
        /// Gets a value indicating whether there are any errors.
        /// </summary>
        public bool HasErrors => Plain.Count > 0 || Labeled.Count > 0;

        /// <summary>
        /// Returns an empty error-state string (unused by these tests).
        /// </summary>
        /// <returns>An empty string.</returns>
        public string ErrorState() => string.Empty;
    }
}
