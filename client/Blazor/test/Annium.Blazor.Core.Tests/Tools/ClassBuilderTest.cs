using System.Collections.Generic;
using Annium.Blazor.Core.Tools;
using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Core.Tests.Tools;

/// <summary>
/// Tests for the ClassBuilder functionality
/// </summary>
public class ClassBuilderTest
{
    /// <summary>
    /// Tests that the generic ClassBuilder works correctly with various conditions and value providers
    /// </summary>
    [Fact]
    public void ClassBuilderT_Works()
    {
        // arrange
        var genderClasses = new Dictionary<Gender, string?> { { Gender.Male, "male" }, { Gender.Female, "female" } };
        var cb = ClassBuilder<User>
            .With("plain")
            .With(() => true, "plain-if")
            .With(x => !x.Name.IsNullOrWhiteSpace(), "plain-if-value")
            .With(() => "get")
            .With(() => true, () => "get-if")
            .With(x => !x.Name.IsNullOrWhiteSpace(), () => "get-if-value")
            .With(x => $"{x.Name}_val")
            .With(() => true, x => $"{x.Name}_val-if")
            .With(x => !x.Name.IsNullOrWhiteSpace(), x => $"{x.Name}_val-if-value")
            .With(x => x.Gender, genderClasses);

        // act
        var unnamed = cb.Build(new User());
        var named = cb.Build(new User { Gender = Gender.Female, Name = "x" });

        // assert
        unnamed.Is("plain plain-if get get-if _val _val-if male");
        named.Is("plain plain-if plain-if-value get get-if get-if-value x_val x_val-if x_val-if-value female");
    }

    /// <summary>
    /// Tests that cloning a generic ClassBuilder works correctly
    /// </summary>
    [Fact]
    public void ClassBuilderT_Clone_Works()
    {
        // arrange
        var cb = ClassBuilder<User>.With("plain");

        // act
        var one = cb.Clone().With("one").Build(new User());
        var two = cb.Clone().With("two").Build(new User());

        // assert
        one.Is("plain one");
        two.Is("plain two");
    }

    /// <summary>
    /// Tests that the non-generic ClassBuilder works correctly with various conditions and value providers
    /// </summary>
    [Fact]
    public void ClassBuilder_Works()
    {
        // arrange
        var genderClasses = new Dictionary<Gender, string?> { { Gender.Male, "male" }, { Gender.Female, "female" } };
        var cb = ClassBuilder
            .With("plain")
            .With(() => true, "plain-if")
            .With(() => "get")
            .With(() => false, () => "get-if")
            .With(Gender.Male, genderClasses);

        // act
        var className = cb.Build();

        // assert
        className.Is("plain plain-if get male");
    }

    /// <summary>
    /// Tests that cloning a non-generic ClassBuilder works correctly
    /// </summary>
    [Fact]
    public void ClassBuilder_Clone_Works()
    {
        // arrange
        var cb = ClassBuilder.With("plain");

        // act
        var one = cb.Clone().With("one").Build();
        var two = cb.Clone().With("two").Build();

        // assert
        one.Is("plain one");
        two.Is("plain two");
    }

    /// <summary>
    /// Test user class for ClassBuilder testing
    /// </summary>
    private class User
    {
        /// <summary>
        /// Gets or sets the gender of the user
        /// </summary>
        public Gender Gender { get; set; }

        /// <summary>
        /// Gets or sets the name of the user
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Gender enumeration for testing purposes
    /// </summary>
    private enum Gender : byte
    {
        /// <summary>
        /// Male gender
        /// </summary>
        Male,

        /// <summary>
        /// Female gender
        /// </summary>
        Female,
    }
}
