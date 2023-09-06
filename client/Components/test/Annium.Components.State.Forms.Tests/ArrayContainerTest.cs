using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Components.State.Forms.Tests;

public class ArrayContainerTest : TestBase
{
    public ArrayContainerTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

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
            children.At(j).IsEqual(state.At(x => x[j]));
        var i = 0;
        state.At(x => x[i]).Value.Is(initialValue.At(0));
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        state.IsStatus(Status.None).IsTrue();
        state.HasStatus(Status.None).IsTrue();
        log.IsEmpty();
    }

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
        state.At(x => x[0]).Value.Is(initialValue.At(0));
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        log.IsEmpty();

        // act
        state.Set(otherValue).IsTrue();

        // assert
        state.Value.IsEqual(otherValue);
        state.At(x => x[0]).Value.Is(otherValue.At(0));
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);

        // act
        state.Set(initialValue).IsTrue();

        // assert
        state.Value.IsEqual(initialValue);
        state.At(x => x[0]).Value.Is(initialValue.At(0));
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
        var initialValue = Arrange();
        var otherValue = new List<int> { 4, 2 };
        var state = factory.CreateArray(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.Set(initialValue).IsFalse();

        // assert
        state.Value.IsEqual(initialValue);
        state.At(x => x[0]).Value.Is(initialValue.At(0));
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        log.IsEmpty();

        // act
        state.Set(otherValue).IsTrue();

        // assert
        state.Value.IsEqual(otherValue);
        state.At(x => x[0]).Value.Is(otherValue.At(0));
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);

        // act
        state.Init(otherValue);

        // assert
        state.Value.IsEqual(otherValue);
        state.At(x => x[0]).Value.Is(otherValue.At(0));
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
        var initialValue = Arrange();
        var otherValue = new List<int> { 4, 2 };
        var state = factory.CreateArray(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.Set(otherValue).IsTrue();
        state.At(x => x[0]).SetStatus(Status.Validating);

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
        state.At(x => x[0]).Value.Is(initialValue.At(0));
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        state.IsStatus(Status.None).IsTrue();
        state.HasStatus(Status.None).IsTrue();
        log.Has(3);
    }

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
        state.At(x => x[0]).SetStatus(Status.Validating);

        // assert
        state.IsStatus(Status.None, Status.Validating).IsTrue();
        state.IsStatus(Status.Validating).IsFalse();
        state.HasStatus(Status.None, Status.Validating).IsTrue();
        state.HasStatus(Status.None, Status.Error).IsTrue();
        state.HasStatus(Status.Error).IsFalse();
        log.Has(1);
    }

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

    private List<int> Arrange() => new() { 2, 8 };
}