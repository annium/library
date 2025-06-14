using System;
using System.Collections.Generic;
using System.Reactive;
using Annium.Testing;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Annium.Components.State.Forms.Tests;

/// <summary>
/// Tests for complex nested object state management functionality
/// </summary>
public class ComplexTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the ComplexTest class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public ComplexTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that complex object state initialization works correctly
    /// </summary>
    [Fact]
    public void Init_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();

        // act
        var state = factory.CreateObject(initialValue);
        state.Changed.Subscribe(log.Add);

        // assert
        state.Value.IsEqual(initialValue);
        state.AtObject(x => x.Author).AtAtomic(x => x.Name).Value.Is(initialValue.Author.Name);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        state.IsStatus(Status.None).IsTrue();
        state.HasStatus(Status.None).IsTrue();
        log.IsEmpty();
    }

    /// <summary>
    /// Tests that setting values on complex object state works correctly
    /// </summary>
    [Fact]
    public void Set_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();
        var otherValue = ArrangeOther();
        var state = factory.CreateObject(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.Set(initialValue).IsFalse();

        // assert
        state.Value.IsEqual(initialValue);
        state.AtAtomic(x => x.Name).Value.Is(initialValue.Name);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        log.IsEmpty();

        // act
        state.Set(otherValue).IsTrue();

        // assert
        state.Value.IsEqual(otherValue);
        state.AtAtomic(x => x.Name).Value.Is(otherValue.Name);
        state.AtObject(x => x.Author).Value.IsEqual(otherValue.Author);
        state.AtArray(x => x.Messages).Value.IsEqual(otherValue.Messages);
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);

        // act
        state.Set(initialValue).IsTrue();

        // assert
        state.Value.IsEqual(initialValue);
        state.AtAtomic(x => x.Name).Value.Is(initialValue.Name);
        state.AtObject(x => x.Author).Value.IsEqual(initialValue.Author);
        state.AtArray(x => x.Messages).Value.IsEqual(initialValue.Messages);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsTrue();
        log.Has(2);
    }

    /// <summary>
    /// Tests that resetting complex object state to initial values works correctly
    /// </summary>
    [Fact]
    public void Reset_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();
        var otherValue = ArrangeOther();
        var state = factory.CreateObject(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.Set(otherValue).IsTrue();
        state.AtAtomic(x => x.Name).SetStatus(Status.Validating);

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
        state
            .AtArray(x => x.Messages)
            .AtObject(x => x[0])
            .AtAtomic(x => x.Text)
            .Value.Is(initialValue.Messages.At(0).Text);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        state.IsStatus(Status.None).IsTrue();
        state.HasStatus(Status.None).IsTrue();
        log.Has(3);
    }

    /// <summary>
    /// Tests that status management for complex object state works correctly
    /// </summary>
    [Fact]
    public void Status_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();
        var state = factory.CreateObject(initialValue);
        state.Changed.Subscribe(log.Add);

        // act
        state.AtAtomic(x => x.Name).SetStatus(Status.Validating);

        // assert
        state.IsStatus(Status.None, Status.Validating).IsTrue();
        state.IsStatus(Status.Validating).IsFalse();
        state.HasStatus(Status.None, Status.Validating).IsTrue();
        state.HasStatus(Status.None, Status.Error).IsTrue();
        state.HasStatus(Status.Error).IsFalse();
        log.Has(1);
    }

    /// <summary>
    /// Creates a sample blog object for testing
    /// </summary>
    /// <returns>A blog object with sample data</returns>
    private Blog Arrange() =>
        new()
        {
            Name = "Sample",
            Author = new User { Name = "Max" },
            Messages = [new Message { Text = "one", IsRead = true }, new Message { Text = "two" }],
            EmbeddedDictionary = new Dictionary<string, Dictionary<int, Message>>
            {
                {
                    "a",
                    new Dictionary<int, Message>
                    {
                        {
                            1,
                            new Message { Text = "hey", IsRead = true }
                        },
                    }
                },
            },
        };

    /// <summary>
    /// Creates an alternative blog object for testing
    /// </summary>
    /// <returns>A blog object with different sample data</returns>
    private Blog ArrangeOther() =>
        new()
        {
            Name = "Demo",
            Author = new User { Name = "Lex" },
            Messages =
            [
                new Message { Text = "three", IsRead = true },
                new Message { Text = "four" },
                new Message { Text = "five", IsRead = true },
            ],
        };

    /// <summary>
    /// Represents a blog entity for testing purposes
    /// </summary>
    /// <summary>
    /// Represents a blog entity for testing purposes
    /// </summary>
    private class Blog
    {
        /// <summary>
        /// Gets or sets the blog name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the blog author
        /// </summary>
        public User Author { get; set; } = null!;

        /// <summary>
        /// Gets or sets the list of messages in the blog
        /// </summary>
        public List<Message> Messages { get; set; } = [];

        /// <summary>
        /// Gets or sets the embedded dictionary for testing complex nested structures
        /// </summary>
        public Dictionary<string, Dictionary<int, Message>> EmbeddedDictionary { get; set; } = new();
    }

    /// <summary>
    /// Represents a user entity for testing purposes
    /// </summary>
    /// <summary>
    /// Represents a user entity for testing purposes
    /// </summary>
    private class User
    {
        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a message entity for testing purposes
    /// </summary>
    private class Message
    {
        /// <summary>
        /// Gets or sets the message text
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the message has been read
        /// </summary>
        public bool IsRead { get; set; }
    }
}
