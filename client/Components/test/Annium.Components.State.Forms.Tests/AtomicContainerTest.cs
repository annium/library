using System;
using System.Collections.Generic;
using System.Reactive;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Components.State.Forms.Tests;

public class AtomicContainerTest : TestBase
{
    public AtomicContainerTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

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