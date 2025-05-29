using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Annium.Testing;
using Xunit;

namespace Annium.Components.State.Forms.Tests;

public class MapContainerTest : TestBase
{
    public MapContainerTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Fact]
    public void Create_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();

        // act
        var state = factory.CreateMap(initialValue);
        state.Changed.Subscribe(log.Add);

        // assert
        state.Value.IsEqual(initialValue);
        var value = state.Value;
        foreach (var itemKey in initialValue.Keys)
            value[itemKey].Is(state.AtAtomic(x => x[itemKey]).Value);
        var key = initialValue.Keys.First();
        state.AtAtomic(x => x[key]).Value.Is(initialValue[key]);
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
        var otherValue = ArrangeOther();
        var state = factory.CreateMap(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.Set(initialValue).IsFalse();

        // assert
        state.Value.IsEqual(initialValue);
        state.AtAtomic(x => x["a"]).Value.Is(initialValue["a"]);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        log.IsEmpty();

        // act
        state.Set(otherValue).IsTrue();

        // assert
        state.Value.IsEqual(otherValue);
        state.AtAtomic(x => x["c"]).Value.Is(otherValue["c"]);
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);

        // act
        state.Set(initialValue).IsTrue();

        // assert
        state.Value.IsEqual(initialValue);
        state.AtAtomic(x => x["a"]).Value.Is(initialValue["a"]);
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
        var otherValue = ArrangeOther();
        var state = factory.CreateMap(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.Set(initialValue).IsFalse();

        // assert
        state.Value.IsEqual(initialValue);
        state.AtAtomic(x => x["a"]).Value.Is(initialValue["a"]);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        log.IsEmpty();

        // act
        state.Set(otherValue).IsTrue();

        // assert
        state.Value.IsEqual(otherValue);
        state.AtAtomic(x => x["c"]).Value.Is(otherValue["c"]);
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);

        // act
        state.Init(otherValue);

        // assert
        state.Value.IsEqual(otherValue);
        state.AtAtomic(x => x["a"]).Value.Is(otherValue["a"]);
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
        var otherValue = ArrangeOther();
        var state = factory.CreateMap(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.Set(otherValue).IsTrue();
        state.AtAtomic(x => x["c"]).SetStatus(Status.Validating);

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
        state.AtAtomic(x => x["c"]).Value.Is(initialValue["c"]);
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
        var state = factory.CreateMap(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.AtAtomic(x => x["a"]).SetStatus(Status.Validating);

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
        var state = factory.CreateMap(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.Add("d", 7);

        // assert
        state.Value.IsEqual(
            new Dictionary<string, int>
            {
                { "a", 2 },
                { "b", 4 },
                { "c", 8 },
                { "d", 7 },
            }
        );
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);
    }

    [Fact]
    public void Remove_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();
        var state = factory.CreateMap(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.Remove("b");

        // assert
        state.Value.IsEqual(new Dictionary<string, int> { { "a", 2 }, { "c", 8 } });
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
        var state = factory.CreateMap(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.Remove("a");
        state.Add("e", 9);
        state.Set(initialValue).IsTrue();

        // assert
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsTrue();
        log.Has(3);
    }

    private Dictionary<string, int> Arrange() =>
        new()
        {
            { "a", 2 },
            { "b", 4 },
            { "c", 8 },
        };

    private Dictionary<string, int> ArrangeOther() => new() { { "a", 2 }, { "c", 4 } };
}
