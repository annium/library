using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Annium.Testing;
using Xunit;

namespace Annium.Components.State.Forms.Tests;

/// <summary>
/// Tests for array container functionality including creation, modification, status tracking, and change notifications.
/// </summary>
public class ArrayContainerTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the ArrayContainerTest class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging.</param>
    public ArrayContainerTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that creating an array container correctly initializes state and child containers.
    /// </summary>
    [Fact]
    public void Create_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();

        // act
        var state = factory.CreateArray(initialValue);
        state.Changed.Subscribe(log.Add);

        // assert
        state.Value.IsEqual(initialValue);
        var children = state.Children;
        foreach (var j in Enumerable.Range(0, children.Count))
            children.At(j).IsEqual(state.AtAtomic(x => x[j]));
        var i = 0;
        state.AtAtomic(x => x[i]).Value.Is(initialValue.At(0));
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        state.IsStatus(Status.None).IsTrue();
        state.HasStatus(Status.None).IsTrue();
        log.IsEmpty();
    }

    /// <summary>
    /// Tests that setting array values correctly updates state and triggers change notifications.
    /// </summary>
    [Fact]
    public void Set_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();
        var otherValue = new List<int> { 4, 2 };
        var state = factory.CreateArray(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.Set(initialValue).IsFalse();

        // assert
        state.Value.IsEqual(initialValue);
        state.AtAtomic(x => x[0]).Value.Is(initialValue.At(0));
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        log.IsEmpty();

        // act
        state.Set(otherValue).IsTrue();

        // assert
        state.Value.IsEqual(otherValue);
        state.AtAtomic(x => x[0]).Value.Is(otherValue.At(0));
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);

        // act
        state.Set(initialValue).IsTrue();

        // assert
        state.Value.IsEqual(initialValue);
        state.AtAtomic(x => x[0]).Value.Is(initialValue.At(0));
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsTrue();
        log.Has(2);
    }

    /// <summary>
    /// Tests that initializing array values correctly resets change tracking state.
    /// </summary>
    [Fact]
    public void Init_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();
        var otherValue = new List<int> { 4, 2 };
        var state = factory.CreateArray(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.Set(initialValue).IsFalse();

        // assert
        state.Value.IsEqual(initialValue);
        state.AtAtomic(x => x[0]).Value.Is(initialValue.At(0));
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        log.IsEmpty();

        // act
        state.Set(otherValue).IsTrue();

        // assert
        state.Value.IsEqual(otherValue);
        state.AtAtomic(x => x[0]).Value.Is(otherValue.At(0));
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);

        // act
        state.Init(otherValue);

        // assert
        state.Value.IsEqual(otherValue);
        state.AtAtomic(x => x[0]).Value.Is(otherValue.At(0));
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        log.Has(2);
    }

    /// <summary>
    /// Tests that resetting an array container correctly restores initial state and clears statuses.
    /// </summary>
    [Fact]
    public void Reset_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();
        var otherValue = new List<int> { 4, 2 };
        var state = factory.CreateArray(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.Set(otherValue).IsTrue();
        state.AtAtomic(x => x[0]).SetStatus(Status.Validating);

        // assert
        state.Value.IsEqual(otherValue);
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        state.IsStatus(Status.None, Status.Validating).IsTrue();
        state.HasStatus(Status.Validating).IsTrue();
        log.Has(2);

        // act
        state.Reset();

        // assert
        state.Value.IsEqual(initialValue);
        state.AtAtomic(x => x[0]).Value.Is(initialValue.At(0));
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        state.IsStatus(Status.None).IsTrue();
        state.HasStatus(Status.None).IsTrue();
        log.Has(3);
    }

    /// <summary>
    /// Tests that status propagation from child containers to parent array container works correctly.
    /// </summary>
    [Fact]
    public void Status_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();
        var state = factory.CreateArray(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.AtAtomic(x => x[0]).SetStatus(Status.Validating);

        // assert
        state.IsStatus(Status.None, Status.Validating).IsTrue();
        state.IsStatus(Status.Validating).IsFalse();
        state.HasStatus(Status.None, Status.Validating).IsTrue();
        state.HasStatus(Status.None, Status.Error).IsTrue();
        state.HasStatus(Status.Error).IsFalse();
        log.Has(1);
    }

    /// <summary>
    /// Tests that adding elements to the array correctly updates state and triggers change notifications.
    /// </summary>
    [Fact]
    public void Add_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();
        var state = factory.CreateArray(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.Add(10);

        // assert
        state.Value.IsEqual(new[] { 2, 8, 10 });
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);
    }

    /// <summary>
    /// Tests that inserting elements at specific positions correctly updates state and triggers change notifications.
    /// </summary>
    [Fact]
    public void Insert_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();
        var state = factory.CreateArray(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.Insert(0, 10);

        // assert
        state.Value.IsEqual(new[] { 10, 2, 8 });
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);
    }

    /// <summary>
    /// Tests that removing elements at specific positions correctly updates state and triggers change notifications.
    /// </summary>
    [Fact]
    public void RemoveAt_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();
        var state = factory.CreateArray(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.RemoveAt(1);

        // assert
        state.Value.IsEqual(new[] { 2 });
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);
    }

    /// <summary>
    /// Tests that change tracking correctly identifies when array state returns to original values.
    /// </summary>
    [Fact]
    public void HasChanged_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();
        var state = factory.CreateArray(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.RemoveAt(0);
        state.Add(1);
        state.Set(initialValue).IsTrue();

        // assert
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsTrue();
        log.Has(3);
    }

    /// <summary>
    /// Tests that Set shrinking the array by more than one item in a single call removes the correct items
    /// without throwing (regression: the removal loop must not index an already-shrinking list ascendingly).
    /// </summary>
    [Fact]
    public void Set_ShrinkByMultipleItems_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var state = factory.CreateArray(new List<int> { 1, 2, 3, 4 });
        state.Changed.Subscribe(log.Add);

        // act: shrink from 4 items to 1 in a single Set (removes 3 items at once)
        state.Set(new List<int> { 9 }).IsTrue();

        // assert: no exception; only the single new item remains
        state.Value.IsEqual(new[] { 9 });
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);
    }

    /// <summary>
    /// Tests that Init shrinking the array by more than one item in a single call removes the correct items
    /// without throwing and resets change tracking.
    /// </summary>
    [Fact]
    public void Init_ShrinkByMultipleItems_Ok()
    {
        // arrange
        var factory = GetFactory();
        var state = factory.CreateArray(new List<int> { 1, 2, 3, 4 });

        // act: shrink from 4 items to 2 in a single Init (removes 2 items at once)
        state.Init(new List<int> { 7, 8 });

        // assert
        state.Value.IsEqual(new[] { 7, 8 });
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
    }

    /// <summary>
    /// Tests that accessing an out-of-range array index via AtAtomic throws an exception.
    /// </summary>
    [Fact]
    public void AtAtomic_IndexOutOfRange_Throws()
    {
        // arrange
        var factory = GetFactory();
        var state = factory.CreateArray(Arrange());

        // assert
        Wrap.It(() => state.AtAtomic(x => x[5])).Throws<IndexOutOfRangeException>();
    }

    /// <summary>
    /// Tests that RemoveAt disposes the removed child's change subscription, so mutating the now-detached child
    /// no longer notifies the parent array (regression: RemoveAt must not leak the child subscription).
    /// </summary>
    [Fact]
    public void RemoveAt_DisposesRemovedChildSubscription()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var state = factory.CreateArray(new List<int> { 2, 8 });
        var removed = state.AtAtomic(x => x[0]);
        state.Changed.Subscribe(log.Add);

        // act
        state.RemoveAt(0);
        log.Clear();
        removed.Set(999);

        // assert: the removed child's subscription was disposed → no parent notification fires
        log.IsEmpty();
    }

    /// <summary>
    /// Tests that touching a child state directly (bypassing the container's own Add/Set/Init/RemoveAt) still
    /// marks the parent array as touched (pins the `_states.Any(x => x.Ref.HasBeenTouched)` OR-branch).
    /// </summary>
    [Fact]
    public void HasBeenTouched_ChildTouchedDirectly_Ok()
    {
        // arrange
        var factory = GetFactory();
        var state = factory.CreateArray(Arrange());

        // act
        state.AtAtomic(x => x[0]).Set(99);

        // assert
        state.HasBeenTouched.IsTrue();
    }

    /// <summary>
    /// Tests that a non-indexer expression (e.g. a property access) passed to AtAtomic throws an
    /// ArgumentException, distinct from the IndexOutOfRangeException thrown for a valid-but-out-of-range index.
    /// </summary>
    [Fact]
    public void AtAtomic_NonIndexExpression_Throws()
    {
        // arrange
        var factory = GetFactory();
        var state = factory.CreateArray(Arrange());

        // assert
        Wrap.It(() => state.AtAtomic(x => x.Count)).Throws<ArgumentException>();
    }

    /// <summary>
    /// Creates a test array with initial values for testing.
    /// </summary>
    /// <returns>A list containing test integer values.</returns>
    private List<int> Arrange() => [2, 8];
}
