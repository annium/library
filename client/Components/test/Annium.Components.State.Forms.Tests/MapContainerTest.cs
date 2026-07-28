using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Annium.Testing;
using Xunit;

namespace Annium.Components.State.Forms.Tests;

/// <summary>
/// Tests for map/dictionary container state management functionality
/// </summary>
public class MapContainerTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the MapContainerTest class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public MapContainerTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that map container creation works correctly
    /// </summary>
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

    /// <summary>
    /// Tests that setting values on map container state works correctly
    /// </summary>
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

    /// <summary>
    /// Tests that initializing map container state works correctly
    /// </summary>
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

    /// <summary>
    /// Tests that resetting map container state to initial values works correctly
    /// </summary>
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

    /// <summary>
    /// Tests that status management for map container state works correctly
    /// </summary>
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

    /// <summary>
    /// Tests that adding items to map container state works correctly
    /// </summary>
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

    /// <summary>
    /// Tests that removing items from map container state works correctly
    /// </summary>
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

    /// <summary>
    /// Tests that change tracking for map container state works correctly
    /// </summary>
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

    /// <summary>
    /// Tests that accessing a non-existent key via AtAtomic throws an exception
    /// </summary>
    [Fact]
    public void AtAtomic_MissingKey_Throws()
    {
        // arrange
        var factory = GetFactory();
        var state = factory.CreateMap(Arrange());

        // assert
        Wrap.It(() => state.AtAtomic(x => x["z"])).Throws<IndexOutOfRangeException>();
    }

    /// <summary>
    /// Tests that Keys reflects the current set of dictionary keys as items are added and removed
    /// </summary>
    [Fact]
    public void Keys_ReflectsCurrentState()
    {
        // arrange
        var factory = GetFactory();
        var initialValue = Arrange();
        var state = factory.CreateMap(initialValue);

        // assert
        state.Keys.IsEqual(initialValue.Keys.ToArray());

        // act
        state.Add("d", 7);

        // assert
        state.Keys.IsEqual(new[] { "a", "b", "c", "d" });

        // act
        state.Remove("a");

        // assert
        state.Keys.IsEqual(new[] { "b", "c", "d" });
    }

    /// <summary>
    /// Tests that touching a value state directly (bypassing the container's own Add/Set/Init/Remove) still
    /// marks the parent map as touched (pins the `_states.Values.Any(x => x.Ref.HasBeenTouched)` OR-branch).
    /// </summary>
    [Fact]
    public void HasBeenTouched_ChildTouchedDirectly_Ok()
    {
        // arrange
        var factory = GetFactory();
        var state = factory.CreateMap(Arrange());

        // act
        state.AtAtomic(x => x["a"]).Set(99);

        // assert
        state.HasBeenTouched.IsTrue();
    }

    /// <summary>
    /// Tests that a non-indexer expression (e.g. a property access) passed to AtAtomic throws an
    /// ArgumentException, distinct from the IndexOutOfRangeException thrown for a missing key.
    /// </summary>
    [Fact]
    public void AtAtomic_NonIndexExpression_Throws()
    {
        // arrange
        var factory = GetFactory();
        var state = factory.CreateMap(Arrange());

        // assert
        Wrap.It(() => state.AtAtomic(x => x.Count)).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that Remove disposes the removed value's change subscription, so mutating the now-detached child
    /// no longer notifies the parent map (regression: Remove must not leak the child subscription).
    /// </summary>
    [Fact]
    public void Remove_DisposesRemovedChildSubscription()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var state = factory.CreateMap(Arrange());
        var removed = state.AtAtomic(x => x["b"]);
        state.Changed.Subscribe(log.Add);

        // act
        state.Remove("b");
        log.Clear();
        removed.Set(999);

        // assert: the removed child's subscription was disposed → no parent notification fires
        log.IsEmpty();
    }

    /// <summary>
    /// Creates a sample dictionary for testing
    /// </summary>
    /// <returns>A dictionary with sample data</returns>
    private Dictionary<string, int> Arrange() =>
        new()
        {
            { "a", 2 },
            { "b", 4 },
            { "c", 8 },
        };

    /// <summary>
    /// Creates an alternative dictionary for testing
    /// </summary>
    /// <returns>A dictionary with different sample data</returns>
    private Dictionary<string, int> ArrangeOther() => new() { { "a", 2 }, { "c", 4 } };
}
