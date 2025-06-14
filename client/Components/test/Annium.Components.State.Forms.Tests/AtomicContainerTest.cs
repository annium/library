using System;
using System.Collections.Generic;
using System.Reactive;
using Annium.Testing;
using Xunit;

namespace Annium.Components.State.Forms.Tests;

/// <summary>
/// Tests for atomic container functionality including creation, value setting, initialization, and reset operations.
/// </summary>
public class AtomicContainerTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the AtomicContainerTest class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging.</param>
    public AtomicContainerTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that creating an atomic container correctly initializes with the provided value.
    /// </summary>
    [Fact]
    public void Create_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();

        // act
        var state = factory.CreateAtomic(5);
        state.Changed.Subscribe(log.Add);

        // assert
        state.Value.Is(5);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        log.IsEmpty();
    }

    /// <summary>
    /// Tests that setting values correctly updates state and triggers change notifications.
    /// </summary>
    [Fact]
    public void Set_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initial = 5;
        var other = 10;
        var state = factory.CreateAtomic(initial);
        state.Changed.Subscribe(log.Add);

        // act
        state.Set(other).IsTrue();

        // assert
        state.Value.Is(other);
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);

        // act
        state.Set(initial).IsTrue();

        // assert
        state.Value.Is(initial);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsTrue();
        log.Has(2);
    }

    /// <summary>
    /// Tests that initializing with a value correctly resets change tracking state.
    /// </summary>
    [Fact]
    public void Init_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initial = 5;
        var other = 10;
        var state = factory.CreateAtomic(initial);
        state.Changed.Subscribe(log.Add);

        // act
        state.Set(other).IsTrue();

        // assert
        state.Value.Is(other);
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);

        // act
        state.Init(other);

        // assert
        state.Value.Is(other);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        log.Has(2);
    }

    /// <summary>
    /// Tests that resetting an atomic container correctly restores the initial value and state.
    /// </summary>
    [Fact]
    public void Reset_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initial = 5;
        var other = 10;
        var state = factory.CreateAtomic(initial);
        state.Changed.Subscribe(log.Add);
        state.Set(other).IsTrue();

        // act
        state.Reset();

        // assert
        state.Value.Is(initial);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        log.Has(2);
    }
}
