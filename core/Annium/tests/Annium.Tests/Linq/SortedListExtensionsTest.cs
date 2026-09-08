using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Linq;

/// <summary>
/// Contains unit tests for SortedList extension methods.
/// </summary>
public class SortedListExtensionsTest
{
    /// <summary>
    /// Verifies that Merge combines both sides in key order and refuses a key the source already holds.
    /// </summary>
    [Fact]
    public void Merge()
    {
        // arrange
        var data = Enumerable.Range(1, 5).Reverse().ToSortedList(x => x);

        // act & assert - duplicate throws
        Wrap.It(() => data.Merge(Enumerable.Range(5, 7).ToDictionary(x => x)))
            .Throws<InvalidOperationException>()
            .Reports("duplicate key 5");

        // act
        var merged = data.Merge(Enumerable.Range(6, 2).ToDictionary(x => x, x => x - 2));

        // assert - the source is left as it was, and the result holds both
        data.Count.Is(5);
        merged.Count.Is(7);
        merged.Keys.IsEqual(Enumerable.Range(1, 7));
        merged.Values.IsEqual(new[] { 1, 2, 3, 4, 5, 4, 5 });
    }

    /// <summary>
    /// Verifies that Merge orders a range that interleaves with, and runs below, what the source holds -
    /// which is what a cache loading history behind what it already has asks for.
    /// </summary>
    [Fact]
    public void Merge_RangeBelowAndBetweenTheSource()
    {
        // arrange
        var data = new[] { 4, 8 }.ToSortedList(x => x);

        // act
        var merged = data.Merge(new[] { 6, 1, 2 }.ToDictionary(x => x, x => x * 10));

        // assert
        merged.Keys.IsEqual(new[] { 1, 2, 4, 6, 8 });
        merged.Values.IsEqual(new[] { 10, 20, 4, 60, 8 });
    }

    /// <summary>
    /// Verifies that merging nothing is answered with the list itself.
    /// </summary>
    [Fact]
    public void Merge_EmptyRange()
    {
        // arrange
        var data = Enumerable.Range(1, 3).ToSortedList(x => x);

        // act
        var merged = data.Merge(new Dictionary<int, int>());

        // assert
        merged.Is(data);
    }

    /// <summary>
    /// Verifies that SetRange works correctly for setting multiple items.
    /// </summary>
    [Fact]
    public void SetRange()
    {
        // arrange
        var data = Enumerable.Range(5, 5).Reverse().ToSortedList(x => x);

        // act
        data.SetRange(Enumerable.Range(9, 3).Reverse().ToDictionary(x => x, x => x - 2));

        // assert
        data.Count.Is(7);
        data.Keys.IsEqual(Enumerable.Range(5, 7));
        data.Values.IsEqual(new[] { 5, 6, 7, 8, 7, 8, 9 });

        // act
        data.SetRange(Enumerable.Range(3, 3).Reverse().ToDictionary(x => x, x => x + 10));

        // assert
        data.Count.Is(9);
        data.Keys.IsEqual(Enumerable.Range(3, 9));
        data.Values.IsEqual(new[] { 13, 14, 15, 6, 7, 8, 7, 8, 9 });
    }

    /// <summary>
    /// Verifies that GetRange works correctly for retrieving a range of items.
    /// </summary>
    [Fact]
    public void GetRange()
    {
        // arrange
        var data = Enumerable.Range(1, 5).ToSortedList(x => x);

        // act & assert - invalid range
        var span = data.GetRange(0, 2);
        span.IsDefault();

        // act & assert - move forward
        span = data.GetRange(1, 2)!;
        span.IsNotDefault();
        span.Count.Is(2);
        span.Move(-1).IsFalse();
        span[0].Is(new(1, 1));
        span[1].Is(new(2, 2));
        span.Move(3).IsTrue();
        span[0].Is(new(4, 4));
        span[1].Is(new(5, 5));

        // act & assert - move backward
        span = data.GetRange(4, 5)!;
        span.IsNotDefault();
        span.Count.Is(2);
        span.Move(1).IsFalse();
        span[0].Is(new(4, 4));
        span[1].Is(new(5, 5));
        span.Move(-3).IsTrue();
        span[0].Is(new(1, 1));
        span[1].Is(new(2, 2));
    }

    /// <summary>
    /// Verifies that FindIndex works correctly for finding indices by key or predicate.
    /// </summary>
    [Fact]
    public void FindIndex()
    {
        // arrange
        var data = Enumerable.Range(1, 5).ToSortedList(x => x);

        // act & assert - by key
        data.FindIndex(0).Is(-1);
        data.FindIndex(1).Is(0);
        data.FindIndex(5).Is(4);
        data.FindIndex(6).Is(-1);

        // act & assert - by value predicate
        data.FindIndex(x => x == 0).Is(-1);
        data.FindIndex(x => x == 1).Is(0);
        data.FindIndex(x => x == 5).Is(4);
        data.FindIndex(x => x == 6).Is(-1);
    }

    /// <summary>
    /// Verifies that GetChunks works correctly for retrieving chunks of data.
    /// </summary>
    [Fact]
    public void GetChunks()
    {
        // arrange
        var data = Enumerable.Range(1, 5).ToSortedList(x => x);

        // act & assert - missing beginning
        var chunks = data.GetChunks(0, 2, Next);
        chunks.Has(2);
        chunks.At((0, 0)).IsDefault();
        chunks.At((1, 2)).Is(data.GetRange(1, 2));

        // act & assert - missing end
        chunks = data.GetChunks(3, 6, Next);
        chunks.Has(2);
        chunks.At((3, 5)).Is(data.GetRange(3, 5));
        chunks.At((6, 6)).IsDefault();

        // act & assert - missing parts
        data = new[] { 1, 3, 5 }.ToSortedList(x => x);
        chunks = data.GetChunks(0, 6, Next);
        chunks.Has(7);
        chunks.At((0, 0)).IsDefault();
        chunks.At((1, 1)).Is(data.GetRange(1, 1));
        chunks.At((2, 2)).IsDefault();
        chunks.At((3, 3)).Is(data.GetRange(3, 3));
        chunks.At((4, 4)).IsDefault();
        chunks.At((5, 5)).Is(data.GetRange(5, 5));
        chunks.At((6, 6)).IsDefault();

        // act & assert - chunk size control
        data = new[] { 1, 3, 4, 6 }.ToSortedList(x => x);
        chunks = data.GetChunks(0, 7, Next, 2);
        chunks.Has(3);
        chunks.At((0, 2)).IsDefault();
        chunks.At((3, 4)).Is(data.GetRange(3, 4));
        chunks.At((5, 7)).IsDefault();

        // act & assert - chunk size control - skip all chunks
        data = new[] { 1, 3, 4, 6 }.ToSortedList(x => x);
        chunks = data.GetChunks(0, 7, Next, 3);
        chunks.Has(1);
        chunks.At((0, 7)).IsDefault();

        // act & assert - chunk size control - resolve chunk if matches all data
        data = new[] { 1, 2, 3 }.ToSortedList(x => x);
        chunks = data.GetChunks(1, 3, Next, 4);
        chunks.Has(1);
        chunks.At((1, 3)).Is(data.GetRange(1, 3));

        static int Next(int x) => x + 1;
    }

    /// <summary>
    /// Verifies that GetChunks throws ArgumentException when start is greater than end.
    /// </summary>
    [Fact]
    public void GetChunks_StartGreaterThanEnd_ThrowsArgumentException()
    {
        // arrange
        var data = Enumerable.Range(1, 5).ToSortedList(x => x);

        // act & assert
        Wrap.It(() => data.GetChunks(5, 3, x => x + 1)).Throws<ArgumentException>();
    }
}
