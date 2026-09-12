using System.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Operations.Tests;

/// <summary>
/// Tests for Result functionality including error handling, data management, and cloning.
/// </summary>
public class ResultTest
{
    /// <summary>
    /// Tests that a blank Result has no errors.
    /// </summary>
    [Fact]
    public void Blank_HasNoErrors()
    {
        // arrange
        var result = Result.Create();

        // assert
        result.IsOk.IsTrue();
    }

    /// <summary>
    /// Tests that a blank Result with data correctly stores the data.
    /// </summary>
    [Fact]
    public void Blank_WithData_IsCorrect()
    {
        // arrange
        var result = Result.Create(5);

        // assert
        result.IsOk.IsTrue();
        result.Data.Is(5);
    }

    /// <summary>
    /// Tests that clearing a Result removes all errors.
    /// </summary>
    [Fact]
    public void Clear_RemovesErrors()
    {
        // arrange
        var result = Result.Create().Error("plain").Error("label", "value");

        // act
        result.Clear();

        // assert
        result.IsOk.IsTrue();
    }

    /// <summary>
    /// Tests that plain errors are correctly added to the PlainErrors collection.
    /// </summary>
    [Fact]
    public void PlainError_IsAddedToPlainErrors()
    {
        // arrange
        var result = Result.Create();

        // act
        result.Error("plain");

        // assert
        result.HasErrors.IsTrue();
        result.PlainErrors.Has(1);
        result.PlainErrors.At(0).Is("plain");
    }

    /// <summary>
    /// Tests that labeled errors are correctly added to the LabeledErrors collection.
    /// </summary>
    [Fact]
    public void LabeledError_IsAddedToLabeledErrors()
    {
        // arrange
        var result = Result.Create();

        // act
        result.Error("label", "plain");

        // assert
        result.HasErrors.IsTrue();
        result.LabeledErrors.Count.Is(1);
        result.LabeledErrors.At("label").At(0).Is("plain");
    }

    /// <summary>
    /// Tests that multiple plain errors can be added using params syntax with duplicates removed.
    /// </summary>
    [Fact]
    public void PlainErrors_Params_IsAddedCorrectly()
    {
        // arrange
        var result = Result.Create();

        // act
        result.Errors("plain", "another", "another");

        // assert
        result.PlainErrors.Has(2);
        result.PlainErrors.At(0).Is("plain");
        result.PlainErrors.At(1).Is("another");
    }

    /// <summary>
    /// Tests that multiple plain errors can be added using collection syntax with duplicates removed.
    /// </summary>
    [Fact]
    public void PlainErrors_Collection_IsAddedCorrectly()
    {
        // arrange
        var result = Result.Create();

        // act
        result.Errors(new List<string> { "plain", "another", "another" });

        // assert
        result.PlainErrors.Has(2);
        result.PlainErrors.At(0).Is("plain");
        result.PlainErrors.At(1).Is("another");
    }

    /// <summary>
    /// Tests that multiple labeled errors can be added using params syntax with proper grouping.
    /// </summary>
    [Fact]
    public void LabeledErrors_Params_IsAddedCorrectly()
    {
        // arrange
        var result = Result.Create();

        // act
        result.Errors(("label", new[] { "plain" }), ("other", new[] { "prev" }), ("other", new[] { "another" }));

        // assert
        result.LabeledErrors.Count.Is(2);
        result.LabeledErrors.At("label").At(0).Is("plain");
        result.LabeledErrors.At("other").At(0).Is("prev");
        result.LabeledErrors.At("other").At(1).Is("another");
    }

    /// <summary>
    /// Tests that multiple labeled errors can be added using collection syntax.
    /// </summary>
    [Fact]
    public void LabeledErrors_Collection_IsAddedCorrectly()
    {
        // arrange
        var result = Result.Create();

        // act
        result.Errors(
            new Dictionary<string, IReadOnlyCollection<string>>
            {
                { "label", new[] { "plain" } },
                { "other", new[] { "another" } },
            }
        );

        // assert
        result.LabeledErrors.Count.Is(2);
        result.LabeledErrors.At("label").At(0).Is("plain");
        result.LabeledErrors.At("other").At(0).Is("another");
    }

    /// <summary>
    /// Tests that joining multiple Results using params syntax correctly merges all errors.
    /// </summary>
    [Fact]
    public void Join_Params_IsDoneCorrectly()
    {
        // arrange
        var result = Result.Create().Error("own").Error("label", "mine");
        var plain = Result.Create().Errors("plain", "another");
        var labeled = Result.Create().Errors(("a", new[] { "va" }), ("b", new[] { "vb" }));

        // act
        result.Join(plain, labeled);

        // assert
        result.HasErrors.IsTrue();
        result.PlainErrors.Has(3);
        result.PlainErrors.At(0).Is("own");
        result.PlainErrors.At(1).Is("plain");
        result.PlainErrors.At(2).Is("another");
        result.LabeledErrors.Count.Is(3);
        result.LabeledErrors.At("label").At(0).Is("mine");
        result.LabeledErrors.At("a").At(0).Is("va");
        result.LabeledErrors.At("b").At(0).Is("vb");
    }

    /// <summary>
    /// Tests that joining multiple Results using collection syntax correctly merges all errors.
    /// </summary>
    [Fact]
    public void Join_Collection_IsDoneCorrectly()
    {
        // arrange
        var result = Result.Create().Error("own").Error("label", "mine");
        var plain = Result.Create().Errors("plain", "another");
        var labeled = Result.Create().Errors(("a", new[] { "va" }), ("b", new[] { "vb" }));

        // act
        result.Join(new List<IResult> { plain, labeled });

        // assert
        result.HasErrors.IsTrue();
        result.PlainErrors.Has(3);
        result.PlainErrors.At(0).Is("own");
        result.PlainErrors.At(1).Is("plain");
        result.PlainErrors.At(2).Is("another");
        result.LabeledErrors.Count.Is(3);
        result.LabeledErrors.At("label").At(0).Is("mine");
        result.LabeledErrors.At("a").At(0).Is("va");
        result.LabeledErrors.At("b").At(0).Is("vb");
    }

    /// <summary>
    /// Tests that cloning a Result produces a valid copy with all errors preserved.
    /// </summary>
    [Fact]
    public void Result_Clone_ReturnsValidClone()
    {
        // arrange
        var result = Result.Create().Error("plain").Error("label", "value");

        // act
        var clone = result.Copy();

        // assert
        clone.HasErrors.IsTrue();
        clone.HasErrors.IsTrue();
        clone.PlainErrors.Has(1);
        clone.PlainErrors.At(0).Is("plain");
        clone.LabeledErrors.Has(1);
        clone.LabeledErrors.At("label").Has(1);
        clone.LabeledErrors.At("label").At(0).Is("value");
    }

    /// <summary>
    /// Tests that cloning a Result with data produces a valid copy with all errors and data preserved.
    /// </summary>
    [Fact]
    public void Result_CloneWithData_ReturnsValidClone()
    {
        // arrange
        var result = Result.Create(10).Error("plain").Error("label", "value");

        // act
        var clone = result.Copy();

        // assert
        clone.Data.Is(10);
        clone.HasErrors.IsTrue();
        clone.PlainErrors.Has(1);
        clone.PlainErrors.At(0).Is("plain");
        clone.LabeledErrors.Has(1);
        clone.LabeledErrors.At("label").Has(1);
        clone.LabeledErrors.At("label").At(0).Is("value");
        clone.Data.Is(10);
    }

    /// <summary>
    /// Tests that the static Join method with params correctly merges multiple Results.
    /// </summary>
    [Fact]
    public void JoinStatic_Params_IsDoneCorrectly()
    {
        // arrange
        var result = Result.Create().Error("own").Error("label", "mine");
        var plain = Result.Create().Errors("plain", "another");
        var labeled = Result.Create().Errors(("a", new[] { "va" }), ("b", new[] { "vb" }));

        // act
        var output = Result.Join(result, plain, labeled);

        // assert
        output.HasErrors.IsTrue();
        output.PlainErrors.Has(3);
        output.PlainErrors.At(0).Is("own");
        output.PlainErrors.At(1).Is("plain");
        output.PlainErrors.At(2).Is("another");
        output.LabeledErrors.Count.Is(3);
        output.LabeledErrors.At("label").At(0).Is("mine");
        output.LabeledErrors.At("a").At(0).Is("va");
        output.LabeledErrors.At("b").At(0).Is("vb");
    }

    /// <summary>
    /// Tests that the static Join method with collection correctly merges multiple Results.
    /// </summary>
    [Fact]
    public void JoinStatic_Collection_IsDoneCorrectly()
    {
        // arrange
        var result = Result.Create().Error("own").Error("label", "mine");
        var plain = Result.Create().Errors("plain", "another");
        var labeled = Result.Create().Errors(("a", new[] { "va" }), ("b", new[] { "vb" }));

        // act
        var output = Result.Join(new List<IResult> { result, plain, labeled });

        // assert
        output.HasErrors.IsTrue();
        output.PlainErrors.Has(3);
        output.PlainErrors.At(0).Is("own");
        output.PlainErrors.At(1).Is("plain");
        output.PlainErrors.At(2).Is("another");
        output.LabeledErrors.Count.Is(3);
        output.LabeledErrors.At("label").At(0).Is("mine");
        output.LabeledErrors.At("a").At(0).Is("va");
        output.LabeledErrors.At("b").At(0).Is("vb");
    }

    /// <summary>
    /// Two results that never carried an error are equal.
    /// </summary>
    /// <remarks>
    /// A deliberate change. The error collections and their lock used to be eagerly created fields, and
    /// the record's synthesized equality compares fields — so two distinct results were <em>never</em>
    /// equal, whatever they contained. With the collections deferred to the first error, a result that
    /// has none carries a null field, and two of them compare equal. That is closer to what the type
    /// means than "never equal" was.
    /// </remarks>
    [Fact]
    public void WithoutErrors_ResultsAreEqual()
    {
        Result.Create().Is(Result.Create());
        Result.Create(5).Is(Result.Create(5));
        Result.Create().Error("x").Clear().Equals(Result.Create()).IsFalse();
    }

    /// <summary>
    /// Results that carry errors are compared by the identity of the errors they carry, not by the
    /// errors themselves — so two results with the same errors are still not equal.
    /// </summary>
    /// <remarks>
    /// The other half of the change above, and the reason it is asymmetric: equality became "equal when
    /// neither has errors", not "equal when the errors match". Pinned so that nobody reads the test
    /// above as a promise of structural equality over errors.
    /// </remarks>
    [Fact]
    public void WithErrors_ResultsAreNotEqual_EvenWithTheSameErrors()
    {
        var left = Result.Create().Error("same");
        var right = Result.Create().Error("same");

        left.Equals(right).IsFalse();
        left.PlainErrors.Has(1);
        right.PlainErrors.Has(1);
    }

    /// <summary>
    /// A result that never carried an error reads as empty through every accessor, and clearing it is a
    /// no-op that still returns the instance for chaining.
    /// </summary>
    [Fact]
    public void WithoutErrors_ReadsEmpty_AndClearsToItself()
    {
        var result = Result.Create();

        result.IsOk.IsTrue();
        result.HasErrors.IsFalse();
        result.PlainErrors.IsEmpty();
        result.PlainError.Is(string.Empty);
        result.LabeledErrors.IsEmpty();
        result.ErrorState().Is(Result.Create().ErrorState());
        result.Clear().Is(result);
    }

    /// <summary>
    /// Copying a result that carries no errors produces another that carries none — the path the
    /// deferral exists for, since <c>CloneTo</c> passes empty error collections through <c>Errors</c>.
    /// </summary>
    [Fact]
    public void Copy_OfResultWithoutErrors_StaysWithoutErrors()
    {
        var copy = Result.Create(7).Copy();

        copy.IsOk.IsTrue();
        copy.Data.Is(7);
        copy.PlainErrors.IsEmpty();
        copy.LabeledErrors.IsEmpty();
    }
}
