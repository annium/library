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
        foreach (var j in Enumerable.Range(0, children.Count - 1))
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
    /// Creates a test array with initial values for testing.
    /// </summary>
    /// <returns>A list containing test integer values.</returns>
    private List<int> Arrange() => [2, 8];
}
