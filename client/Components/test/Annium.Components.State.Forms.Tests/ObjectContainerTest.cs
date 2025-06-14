using System;
using System.Collections.Generic;
using System.Reactive;
using Annium.Components.State.Forms.Extensions;
using Annium.Extensions.Validation;
using Annium.Testing;
using Xunit;

namespace Annium.Components.State.Forms.Tests;

/// <summary>
/// Tests for object container state management functionality
/// </summary>
public class ObjectContainerTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the ObjectContainerTest class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public ObjectContainerTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that object container creation works correctly
    /// </summary>
    [Fact]
    public void Create_Ok()
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
        state.Children.At(nameof(User.Age)).Is(state.AtAtomic(x => x.Age));
        state.Children.At(nameof(User.Name)).Is(state.AtAtomic(x => x.Name));
        state.AtAtomic(x => x.Name).Value.Is(initialValue.Name);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        state.IsStatus(Status.None).IsTrue();
        state.HasStatus(Status.None).IsTrue();
        log.IsEmpty();
    }

    /// <summary>
    /// Tests that setting values on object container state works correctly
    /// </summary>
    [Fact]
    public void Set_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();
        var otherValue = new User { Name = "Lex" };
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
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);

        // act
        state.Set(initialValue).IsTrue();

        // assert
        state.Value.IsEqual(initialValue);
        state.AtAtomic(x => x.Name).Value.Is(initialValue.Name);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsTrue();
        log.Has(2);
    }

    /// <summary>
    /// Tests that initializing object container state works correctly
    /// </summary>
    [Fact]
    public void Init_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();
        var otherValue = new User { Name = "Lex" };
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
        state.HasChanged.IsTrue();
        state.HasBeenTouched.IsTrue();
        log.Has(1);

        // act
        state.Init(otherValue);

        // assert
        state.Value.IsEqual(otherValue);
        state.AtAtomic(x => x.Name).Value.Is(otherValue.Name);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        log.Has(2);
    }

    /// <summary>
    /// Tests that resetting object container state to initial values works correctly
    /// </summary>
    [Fact]
    public void Reset_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var initialValue = Arrange();
        var otherValue = new User { Name = "Lex" };
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
        state.AtAtomic(x => x.Name).Value.Is(initialValue.Name);
        state.HasChanged.IsFalse();
        state.HasBeenTouched.IsFalse();
        state.IsStatus(Status.None).IsTrue();
        state.HasStatus(Status.None).IsTrue();
        log.Has(3);
    }

    /// <summary>
    /// Tests that status management for object container state works correctly
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
    /// Tests that validation for object container state works correctly
    /// </summary>
    [Fact]
    public void Validation_Ok()
    {
        // arrange
        var log = new List<Unit>();
        var factory = GetFactory();
        var validator = GetValidator<User>();
        var initialValue = Arrange();
        var state = factory.CreateObject(initialValue);
        state.Changed.Subscribe(log.Add);
        state.UseValidator(validator);

        // act
        state.Set(new User { Age = 10, Name = "No" });

        // assert
        state.HasStatus(Status.Error).IsTrue();

        // act
        state.Set(new User { Age = 20, Name = "Name" });

        // assert
        state.IsStatus(Status.None).IsTrue();

        // assert
        log.Has(2);
    }

    /// <summary>
    /// Creates a sample user object for testing
    /// </summary>
    /// <returns>A user object with sample data</returns>
    private User Arrange() =>
        new()
        {
            Name = "Max",
            Age = 20,
            Sex = Sex.Male,
        };

    /// <summary>
    /// Represents a user entity for testing purposes
    /// </summary>
    private class User
    {
        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user age
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the user sex
        /// </summary>
        public Sex Sex { get; set; }
    }

    /// <summary>
    /// Represents the sex/gender options for testing purposes
    /// </summary>
    private enum Sex
    {
        /// <summary>
        /// Female sex
        /// </summary>
        Female,

        /// <summary>
        /// Male sex
        /// </summary>
        Male,
    }

    /// <summary>
    /// Validator for User objects used in testing
    /// </summary>
    // ReSharper disable once UnusedType.Local
    private class UserValidator : Validator<User>
    {
        /// <summary>
        /// Initializes a new instance of the UserValidator class
        /// </summary>
        public UserValidator()
        {
            Field(x => x.Age).GreaterThan(18);
            Field(x => x.Name).Required().MinLength(3);
        }
    }
}
