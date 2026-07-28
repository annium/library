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
    /// Tests that FALSE <see cref="System.Func{Boolean}"/> predicates exclude their fragment across every
    /// data-independent conditional overload (pins the false branch of each `predicate() ? … : string.Empty`,
    /// which the all-true ClassBuilderT_Works cannot).
    /// </summary>
    [Fact]
    public void ClassBuilderT_FalsePredicate_Excluded()
    {
        // arrange: every Func<bool> predicate is false
        var cb = ClassBuilder<User>
            .With("keep")
            .With(() => false, "no-plain-if")
            .With(() => false, () => "no-get-if")
            .With(() => false, x => $"{x.Name}-no-val-if");

        // act
        var result = cb.Build(new User { Name = "x" });

        // assert: only the unconditional fragment survives
        result.Is("keep");
    }

    /// <summary>
    /// Tests that a dictionary MISS (key absent) contributes nothing (pins the `: string.Empty` fall-back of the
    /// generic dictionary overload, which ClassBuilderT_Works — where every key is present — cannot).
    /// </summary>
    [Fact]
    public void ClassBuilderT_DictionaryMiss_Excluded()
    {
        // arrange: dictionary maps only Male
        var classes = new Dictionary<Gender, string?> { { Gender.Male, "male" } };
        var cb = ClassBuilder<User>.With(x => x.Gender, classes);

        // act + assert: present key resolves, absent key contributes nothing
        cb.Build(new User { Gender = Gender.Male }).Is("male");
        cb.Build(new User { Gender = Gender.Female }).Is(string.Empty);
    }

    /// <summary>
    /// Tests that <c>null</c> and whitespace-only fragments are filtered by Build (pins the
    /// `IsNullOrWhiteSpace` guard — an <c>IsNullOrEmpty</c> mutant would leak the whitespace fragment as a
    /// double space).
    /// </summary>
    [Fact]
    public void ClassBuilderT_NullAndWhitespace_Filtered()
    {
        // arrange: interleave null / whitespace-only fragments between real ones
        var cb = ClassBuilder<User>.With("a").With((string?)null).With("   ").With(x => (string?)null).With("b");

        // act
        var result = cb.Build(new User());

        // assert: no empty segments, no leading/trailing/double spaces
        result.Is("a b");
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
    /// Tests both branches of the non-generic conditional overloads: a FALSE predicate excludes its className,
    /// and a TRUE predicate includes its fetched value (ClassBuilder_Works only exercises true-className and
    /// false-fetch, leaving false-className and true-fetch unpinned).
    /// </summary>
    [Fact]
    public void ClassBuilder_BothPredicateBranches()
    {
        // arrange
        var cb = ClassBuilder.With("keep").With(() => false, "no-plain-if").With(() => true, () => "yes-get-if");

        // act
        var result = cb.Build();

        // assert: false-className dropped, true-fetch kept
        result.Is("keep yes-get-if");
    }

    /// <summary>
    /// Tests that a dictionary MISS (key absent) contributes nothing for the non-generic overload (pins its own
    /// `: string.Empty` fall-back — a separate implementation from the generic one).
    /// </summary>
    [Fact]
    public void ClassBuilder_DictionaryMiss_Excluded()
    {
        // arrange: dictionary maps only Male
        var classes = new Dictionary<Gender, string?> { { Gender.Male, "male" } };

        // act + assert: present key resolves, absent key contributes nothing
        ClassBuilder.With(Gender.Male, classes).Build().Is("male");
        ClassBuilder.With(Gender.Female, classes).Build().Is(string.Empty);
    }

    /// <summary>
    /// Tests that <c>null</c> and whitespace-only fragments are filtered by the non-generic Build (pins its own
    /// `IsNullOrWhiteSpace` guard).
    /// </summary>
    [Fact]
    public void ClassBuilder_NullAndWhitespace_Filtered()
    {
        // arrange: interleave null / whitespace-only fragments between real ones
        var cb = ClassBuilder.With("a").With((string?)null).With("   ").With(() => (string?)null).With("b");

        // act
        var result = cb.Build();

        // assert: no empty segments, no leading/trailing/double spaces
        result.Is("a b");
    }

    /// <summary>
    /// Tests that the non-generic builder's <see cref="object.ToString"/> override returns the built class string
    /// (Blazor markup binds <c>class="@cb"</c> via virtual ToString dispatch; without the override it would emit
    /// the type name).
    /// </summary>
    [Fact]
    public void ClassBuilder_ToString_ReturnsBuild()
    {
        // arrange
        var cb = ClassBuilder.With("a").With(() => true, "b");

        // act + assert: ToString() mirrors Build()
        cb.ToString().Is("a b");
        cb.ToString().Is(cb.Build());
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
