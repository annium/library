using System;
using System.Collections.Generic;
using System.Reactive;
using Annium.Testing;
using Xunit;

namespace Annium.Components.State.Forms.Tests;

public class AtomicContainerTest : TestBase
{
    [Fact]
    public void Init_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();

        // act
        var state = factory.Create(5);
        state.Changed.Subscribe(log.Add);

        // assert
        state.Value.IsEqual(5);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        log.IsEmpty();
    }

    [Fact]
    public void Change_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initial = 5;
        var other = 10;
        var state = factory.Create(initial);
        state.Changed.Subscribe(log.Add);

        // act
        state.Set(other).IsTrue();

        // assert
        state.Value.IsEqual(other);
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);

        // act
        state.Set(initial).IsTrue();

        // assert
        state.Value.IsEqual(initial);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsTrue();
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
        var state = factory.Create(initial);
        state.Changed.Subscribe(log.Add);
        state.Set(other).IsTrue();

        // act
        state.Reset();

        // assert
        state.Value.IsEqual(initial);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        log.Has(2);
    }
}