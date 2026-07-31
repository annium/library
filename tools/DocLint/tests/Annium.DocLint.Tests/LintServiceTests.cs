using System.Collections.Generic;
using System.Linq;
using Annium.DocLint.Internal.Services;
using Annium.Testing;
using Xunit;

namespace Annium.DocLint.Tests;

public class LintServiceTests
{
    private readonly LintService _service = new();

    private IReadOnlyList<string> Lint(string source) => _service.Lint(source, TestContext.Current.CancellationToken);

    [Fact]
    public void Lint_FullyDocumentedType_ReportsNothing()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>A documented type.</summary>
            public class Sample
            {
                /// <summary>A documented field.</summary>
                private int _value;

                /// <summary>A documented property.</summary>
                public int Value { get; set; }

                /// <summary>A documented method.</summary>
                /// <param name="input">The input.</param>
                /// <returns>The output.</returns>
                public string Run(string input) => input;

                /// <summary>A documented void method.</summary>
                public void Reset() { }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public void Lint_NestedTypeMember_ReportsOnceQualifiedByOuterType()
    {
        // arrange — regression: members were collected via DescendantNodes from every enclosing
        // type, so a nested member was reported twice, once against the wrong type name
        const string source = """
            namespace T;

            /// <summary>Outer.</summary>
            public class Outer
            {
                /// <summary>Inner.</summary>
                private class Inner
                {
                    public void Run() { }
                }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors
            .Has(1)
            .At(0)
            .IsEqual("Outer.Inner.Run: Missing documentation. Required blocks: summary, param, returns");
    }

    [Fact]
    public void Lint_MultiVariableFieldDeclaration_ReportsEachVariable()
    {
        // arrange — regression: `Variables.Single()` threw on a multi-variable declaration,
        // aborting the whole run instead of reporting the members
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                private int _a, _b;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(2);
        errors.At(0).IsEqual("Sample._a: Missing documentation. Required blocks: summary");
        errors.At(1).IsEqual("Sample._b: Missing documentation. Required blocks: summary");
    }

    [Fact]
    public void Lint_UndocumentedType_ReportsMissingDocumentation()
    {
        // arrange
        const string source = """
            namespace T;

            public class Sample { }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("Sample: Missing documentation. Required blocks: summary");
    }

    [Fact]
    public void Lint_EmptySummary_ReportsEmptyDocumentation()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary></summary>
            public class Sample { }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("Sample: Missing or empty summary documentation");
    }

    [Fact]
    public void Lint_MethodMissingParamDoc_ReportsParameter()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Runs.</summary>
                /// <returns>The output.</returns>
                public string Run(string input) => input;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("Sample.Run.input: Missing or empty parameter documentation");
    }

    [Fact]
    public void Lint_EscapedKeywordParameter_MatchesDocWithoutAtSign()
    {
        // arrange — `@class` is documented as `class`
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Runs.</summary>
                /// <param name="class">The class.</param>
                /// <returns>The output.</returns>
                public string Run(string @class) => @class;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public void Lint_MethodMissingReturnsDoc_ReportsReturns()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Runs.</summary>
                public string Run() => string.Empty;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("Sample.Run: Missing or empty returns documentation");
    }

    [Fact]
    public void Lint_VoidMethod_DoesNotRequireReturnsDoc()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Runs.</summary>
                public void Run() { }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public void Lint_UndocumentedDelegate_ReportsSummaryParamAndReturns()
    {
        // arrange
        const string source = """
            namespace T;

            public delegate string Handler(int input);
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("Handler: Missing documentation. Required blocks: summary, param, returns");
    }

    [Fact]
    public void Lint_DocumentedDelegate_ReportsNothing()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Handles.</summary>
            /// <param name="input">The input.</param>
            /// <returns>The output.</returns>
            public delegate string Handler(int input);
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public void Lint_DelegateMissingParamDoc_ReportsParameter()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Handles.</summary>
            /// <returns>The output.</returns>
            public delegate string Handler(int input);
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("Handler.input: Missing or empty parameter documentation");
    }

    [Theory]
    [InlineData("struct")]
    [InlineData("interface")]
    [InlineData("record")]
    public void Lint_UndocumentedTypeKinds_AreReported(string keyword)
    {
        // arrange
        var source = $$"""
            namespace T;

            public {{keyword}} Sample { }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("Sample: Missing documentation. Required blocks: summary");
    }

    [Fact]
    public void Lint_SameMemberNameInTwoTypes_ReportsEachAgainstItsOwnType()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>A.</summary>
            public class A
            {
                public void Run() { }
            }

            /// <summary>B.</summary>
            public class B
            {
                public void Run() { }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(2);
        errors.Count(x => x.StartsWith("A.Run:")).Is(1);
        errors.Count(x => x.StartsWith("B.Run:")).Is(1);
    }
}
