using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.DocLint.Internal.Services;
using Annium.Testing;
using Xunit;

namespace Annium.DocLint.Tests;

public class LintServiceTests
{
    private readonly LintService _service = new();

    private IReadOnlyList<string> Lint(string source) =>
        _service.Lint(source, TestContext.Current.CancellationToken).Errors;

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
        errors.Has(1).At(0).IsEqual("T.Outer.Inner.Run(): Missing documentation. Required blocks: summary");
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
        errors.At(0).IsEqual("T.Sample._a: Missing documentation. Required blocks: summary");
        errors.At(1).IsEqual("T.Sample._b: Missing documentation. Required blocks: summary");
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
        errors.Has(1).At(0).IsEqual("T.Sample: Missing documentation. Required blocks: summary");
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
        errors.Has(1).At(0).IsEqual("T.Sample: Missing or empty summary documentation");
    }

    [Fact]
    public void Lint_WhitespaceOnlySummary_ReportsEmptyDocumentation()
    {
        // arrange — a doc block left as a blank template line is what editors actually produce; the
        // literally-empty `<summary></summary>` case cannot tell a whitespace check from a length check
        const string source = """
            namespace T;

            /// <summary>
            /// </summary>
            public class Sample { }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample: Missing or empty summary documentation");
    }

    [Fact]
    public void Lint_SummaryOfNestedElementsOnly_ReportsNothing()
    {
        // arrange — content is taken token by token to see past the `///` exterior trivia, so a block
        // whose text lives entirely inside nested elements must still count as documented
        const string source = """
            namespace T;

            /// <summary>
            /// <see cref="System.String"/>
            /// </summary>
            public class Sample { }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public void Lint_UndocumentedPrimaryConstructorClass_RequiresSummaryAndParam()
    {
        // arrange — every other primary-ctor case is a record, so the check could narrow to records
        // and still pass while under-enforcing the class form this codebase uses for DI
        const string source = """
            namespace T;

            public class Sample(int value) { }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample: Missing documentation. Required blocks: summary, param");
    }

    [Fact]
    public void Lint_UndocumentedPartialType_TagsTheReportWithItsNamespacedKey()
    {
        // arrange — the tag is what lets the command drop this report once another file documents the
        // type; without it the report is indistinguishable from a non-partial type's
        const string source = """
            namespace T;

            public partial class Sample { }
            """;

        // act
        var result = _service.Lint(source, TestContext.Current.CancellationToken);

        // assert
        result.Findings.Has(1).At(0).PartialType.IsEqual("T.Sample");
        result.DocumentedPartialTypes.IsEmpty();
    }

    [Fact]
    public void Lint_DocumentedPartialType_ReportsItAsDocumented()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public partial class Sample { }
            """;

        // act
        var result = _service.Lint(source, TestContext.Current.CancellationToken);

        // assert
        result.Findings.IsEmpty();
        result.DocumentedPartialTypes.Has(1).At(0).IsEqual("T.Sample");
    }

    [Fact]
    public void Lint_PartialTypeInNestedBlockNamespaces_IsKeyedByTheFullNamespace()
    {
        // arrange — every other case uses a file-scoped namespace, whose single declaration carries the
        // whole name; the nested block form splits it, and keying only the innermost segment made
        // App.Server.Types.Foo and App.Client.Types.Foo collide
        const string source = """
            namespace App.Server
            {
                namespace Types
                {
                    /// <summary>Foo.</summary>
                    public partial class Foo { }
                }
            }
            """;

        // act
        var result = _service.Lint(source, TestContext.Current.CancellationToken);

        // assert
        result.DocumentedPartialTypes.Has(1).At(0).IsEqual("App.Server.Types.Foo");
    }

    [Fact]
    public void Lint_SameNamedTypesInTwoNamespaces_ReportsDistinguishableNames()
    {
        // arrange — the file path printed above the errors cannot separate two namespaces inside one
        // file, so the reported name carries the namespace
        const string source = """
            namespace App.Server
            {
                public class Sample { }
            }

            namespace App.Client
            {
                public class Sample { }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(2);
        errors.At(0).IsEqual("App.Server.Sample: Missing documentation. Required blocks: summary");
        errors.At(1).IsEqual("App.Client.Sample: Missing documentation. Required blocks: summary");
    }

    [Fact]
    public void Lint_TypeOutsideAnyNamespace_ReportsItsBareName()
    {
        // arrange — a file with no namespace has nothing to qualify with, and must not gain a stray dot
        const string source = "public class Sample { }";

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("Sample: Missing documentation. Required blocks: summary");
    }

    [Fact]
    public void Lint_NestedPartialType_IsKeyedApartFromTheSameNameSpelledAsANamespace()
    {
        // arrange — `namespace A { class B { class C } }` and `namespace A.B { class C }` are unrelated
        // types whose segments read alike; a `.`-joined key made one silence the other
        const string nested = """
            namespace A;

            /// <summary>B.</summary>
            public partial class B
            {
                /// <summary>C.</summary>
                public partial class C { }
            }
            """;
        const string topLevel = """
            namespace A.B;

            /// <summary>C.</summary>
            public partial class C { }
            """;

        // act
        var nestedResult = _service.Lint(nested, TestContext.Current.CancellationToken);
        var topLevelResult = _service.Lint(topLevel, TestContext.Current.CancellationToken);

        // assert
        nestedResult.DocumentedPartialTypes.Contains("A.B+C").IsTrue();
        topLevelResult.DocumentedPartialTypes.Has(1).At(0).IsEqual("A.B.C");
    }

    [Fact]
    public void Lint_PartialTypeDeclaringAPrimaryConstructor_IsNotSuppressible()
    {
        // arrange — the <param> blocks for these parameters can only live here, so another file's
        // summary must not silence this declaration's report
        const string source = """
            namespace T;

            public partial class Sample(int value) { }
            """;

        // act
        var result = _service.Lint(source, TestContext.Current.CancellationToken);

        // assert
        result.Findings.Has(1).At(0).PartialType.IsDefault();
    }

    [Fact]
    public void Lint_FilePartialType_IsNotKeyedForCrossFileSuppression()
    {
        // arrange — a `file` type is local to its file, so two files each declaring
        // `file partial class Sample` declare two unrelated types; keying them alike let a documented
        // one silence the other
        const string source = """
            namespace T;

            file partial class Sample { }
            """;

        // act
        var result = _service.Lint(source, TestContext.Current.CancellationToken);

        // assert
        result.Findings.Has(1).At(0).PartialType.IsDefault();
        result.DocumentedPartialTypes.IsEmpty();
    }

    [Fact]
    public void Lint_GenericMethodOverloadsDifferingOnlyByArity_ReportsDistinguishableNames()
    {
        // arrange — the value parameters are identical, so a name without the type parameters renders
        // both overloads the same and the reader cannot tell which one is still undocumented
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                public T1 Run<T1>(object value) => default!;

                public T1 Run<T1, T2>(object value) => default!;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(2);
        errors
            .At(0)
            .IsEqual(
                "T.Sample.Run<T1>(object): Missing documentation. Required blocks: summary, typeparam, param, returns"
            );
        errors
            .At(1)
            .IsEqual(
                "T.Sample.Run<T1, T2>(object): Missing documentation. Required blocks: summary, typeparam, param, returns"
            );
    }

    [Fact]
    public void Lint_GenericPartialTypes_AreKeyedByArity()
    {
        // arrange — `Split<T>` and `Split<T, U>` are different types that share an identifier, so a
        // key without arity would let documenting one silence the other's report
        const string source = """
            namespace T;

            /// <summary>One.</summary>
            /// <typeparam name="T1">The first type.</typeparam>
            public partial class Split<T1> { }

            public partial class Split<T1, T2> { }
            """;

        // act
        var result = _service.Lint(source, TestContext.Current.CancellationToken);

        // assert
        result.DocumentedPartialTypes.Has(1).At(0).IsEqual("T.Split`1");
        result.Findings.Has(1).At(0).PartialType.IsEqual("T.Split`2");
    }

    [Fact]
    public void Lint_NonPartialType_IsNotKeyedForCrossFileSuppression()
    {
        // arrange — only a partial type is documented elsewhere; tagging any type would let one file's
        // documentation silence an entirely different type of the same name
        const string source = """
            namespace T;

            public class Sample { }
            """;

        // act
        var result = _service.Lint(source, TestContext.Current.CancellationToken);

        // assert
        result.Findings.Has(1).At(0).PartialType.IsDefault();
        result.DocumentedPartialTypes.IsEmpty();
    }

    [Fact]
    public void Lint_NestedPartialType_IsKeyedByItsContainingType()
    {
        // arrange — the key qualifies by containing types, so two nested partials of the same simple
        // name in one namespace stay distinct
        const string source = """
            namespace T;

            /// <summary>Outer.</summary>
            public partial class Outer
            {
                /// <summary>Inner.</summary>
                public partial class Inner { }
            }
            """;

        // act
        var result = _service.Lint(source, TestContext.Current.CancellationToken);

        // assert
        result.DocumentedPartialTypes.Has(2);
        result.DocumentedPartialTypes.Contains("T.Outer").IsTrue();
        result.DocumentedPartialTypes.Contains("T.Outer+Inner").IsTrue();
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
        errors.Has(1).At(0).IsEqual("T.Sample.Run(string).input: Missing or empty parameter documentation");
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
        errors.Has(1).At(0).IsEqual("T.Sample.Run(): Missing or empty returns documentation");
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
        errors.Has(1).At(0).IsEqual("T.Handler(int): Missing documentation. Required blocks: summary, param, returns");
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
        errors.Has(1).At(0).IsEqual("T.Handler(int).input: Missing or empty parameter documentation");
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
        errors.Has(1).At(0).IsEqual("T.Sample: Missing documentation. Required blocks: summary");
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
        errors.Count(x => x.StartsWith("T.A.Run():")).Is(1);
        errors.Count(x => x.StartsWith("T.B.Run():")).Is(1);
    }

    [Fact]
    public void Lint_UndocumentedEnum_ReportsTypeAndMembers()
    {
        // arrange — enums are not TypeDeclarationSyntax, so they were skipped entirely
        const string source = """
            namespace T;

            public enum Sample
            {
                One,
                Two,
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(3);
        errors.At(0).IsEqual("T.Sample: Missing documentation. Required blocks: summary");
        errors.At(1).IsEqual("T.Sample.One: Missing documentation. Required blocks: summary");
        errors.At(2).IsEqual("T.Sample.Two: Missing documentation. Required blocks: summary");
    }

    [Fact]
    public void Lint_DocumentedEnum_ReportsNothing()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public enum Sample
            {
                /// <summary>One.</summary>
                One,
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public void Lint_UndocumentedConstructor_ReportsSummaryAndParamButNotReturns()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                public Sample(int value) { }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample.Sample(int): Missing documentation. Required blocks: summary, param");
    }

    [Fact]
    public void Lint_DocumentedConstructor_ReportsNothing()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Creates.</summary>
                /// <param name="value">The value.</param>
                public Sample(int value) { }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public void Lint_UndocumentedEvent_ReportsSummary()
    {
        // arrange
        const string source = """
            namespace T;

            using System;

            /// <summary>Sample.</summary>
            public class Sample
            {
                public event Action Changed;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample.Changed: Missing documentation. Required blocks: summary");
    }

    [Fact]
    public void Lint_UndocumentedPrimaryConstructorParameter_ReportsParameterAgainstType()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public record Sample(int Value);
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample.Value: Missing or empty parameter documentation");
    }

    [Fact]
    public void Lint_DocumentedPrimaryConstructorParameter_ReportsNothing()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            /// <param name="Value">The value.</param>
            public record Sample(int Value);
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public void Lint_UndocumentedIndexer_ReportsSummaryParamAndReturns()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                public int this[int index] => index;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors
            .Has(1)
            .At(0)
            .IsEqual("T.Sample.this[int]: Missing documentation. Required blocks: summary, param, returns");
    }

    [Fact]
    public void Lint_UndocumentedOperator_ReportsSummaryParamAndReturns()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                public static bool operator ==(Sample a, Sample b) => true;

                public static bool operator !=(Sample a, Sample b) => false;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(2);
        errors
            .At(0)
            .IsEqual(
                "T.Sample.operator ==(Sample, Sample): Missing documentation. Required blocks: summary, param, returns"
            );
        errors
            .At(1)
            .IsEqual(
                "T.Sample.operator !=(Sample, Sample): Missing documentation. Required blocks: summary, param, returns"
            );
    }

    [Fact]
    public void Lint_NestedEnum_QualifiesByContainingType()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Outer.</summary>
            public class Outer
            {
                public enum Inner
                {
                    One,
                }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(2);
        errors.At(0).IsEqual("T.Outer.Inner: Missing documentation. Required blocks: summary");
        errors.At(1).IsEqual("T.Outer.Inner.One: Missing documentation. Required blocks: summary");
    }

    [Fact]
    public void Lint_UndocumentedConversionOperator_ReportsSummaryParamAndReturns()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                public static implicit operator int(Sample value) => 0;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors
            .Has(1)
            .At(0)
            .IsEqual(
                "T.Sample.implicit operator int(Sample): Missing documentation. Required blocks: summary, param, returns"
            );
    }

    [Fact]
    public void Lint_DocumentedConversionOperator_ReportsNothing()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Converts to an int.</summary>
                /// <param name="value">The value to convert.</param>
                /// <returns>The converted value.</returns>
                public static explicit operator int(Sample value) => 0;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public void Lint_UndocumentedEventWithAccessors_ReportsSummary()
    {
        // arrange — an event declared with explicit add/remove is EventDeclarationSyntax, a different
        // branch from the field-style `public event Action Changed;` covered above
        const string source = """
            using System;

            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                public event Action Changed
                {
                    add { }
                    remove { }
                }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample.Changed: Missing documentation. Required blocks: summary");
    }

    [Fact]
    public void Lint_MethodEmptyParamDoc_ReportsParameter()
    {
        // arrange — the <param> tag is present but carries no content
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Runs.</summary>
                /// <param name="input"></param>
                /// <returns>The output.</returns>
                public string Run(string input) => input;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample.Run(string).input: Missing or empty parameter documentation");
    }

    [Fact]
    public void Lint_MethodEmptyReturnsDoc_ReportsReturns()
    {
        // arrange — the <returns> tag is present but carries no content
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Runs.</summary>
                /// <param name="input">The input.</param>
                /// <returns></returns>
                public string Run(string input) => input;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample.Run(string): Missing or empty returns documentation");
    }

    [Fact]
    public void Lint_DocumentedOperator_ReportsNothing()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Compares for equality.</summary>
                /// <param name="a">Left operand.</param>
                /// <param name="b">Right operand.</param>
                /// <returns>True when equal.</returns>
                public static bool operator ==(Sample a, Sample b) => true;

                /// <summary>Compares for inequality.</summary>
                /// <param name="a">Left operand.</param>
                /// <param name="b">Right operand.</param>
                /// <returns>True when not equal.</returns>
                public static bool operator !=(Sample a, Sample b) => false;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public void Lint_DocumentedIndexer_ReportsNothing()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Gets the value at the index.</summary>
                /// <param name="index">The index.</param>
                /// <returns>The value.</returns>
                public int this[int index] => index;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public async Task LintAsync_File_ReportsSameErrorsAsLintOfItsText()
    {
        // arrange — LintAsync is what LintCommand actually calls; only the string overload was covered
        const string source = """
            namespace T;

            public class Sample
            {
                public string Run(string input) => input;
            }
            """;
        using var sources = TempSources.Create(("Sample.cs", source));

        // act
        var result = await _service.LintAsync(sources.PathOf("Sample.cs"), TestContext.Current.CancellationToken);
        var errors = result.Errors;

        // assert
        errors.Has(2);
        errors.At(0).IsEqual("T.Sample: Missing documentation. Required blocks: summary");
        errors.At(1).IsEqual("T.Sample.Run(string): Missing documentation. Required blocks: summary, param, returns");
    }

    [Fact]
    public void Lint_MethodWithOnlySomeParametersDocumented_ReportsTheUndocumentedOnes()
    {
        // arrange — every other multi-parameter case documents all parameters or none, so a check
        // that stopped after the first parameter would go unnoticed
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Runs.</summary>
                /// <param name="first">The first input.</param>
                /// <returns>The output.</returns>
                public string Run(string first, string second) => first + second;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample.Run(string, string).second: Missing or empty parameter documentation");
    }

    [Fact]
    public void Lint_MultiVariableEventFieldDeclaration_ReportsEachVariable()
    {
        // arrange — the event-field arm loops over its variables just like the field arm, which has
        // its own regression test for a `.Single()` crash
        const string source = """
            using System;

            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                public event Action A, B;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(2);
        errors.At(0).IsEqual("T.Sample.A: Missing documentation. Required blocks: summary");
        errors.At(1).IsEqual("T.Sample.B: Missing documentation. Required blocks: summary");
    }

    [Fact]
    public void Lint_UndocumentedProperty_ReportsSummary()
    {
        // arrange — every other member kind has a negative case; the property branch was only ever
        // reached through the fully-documented happy path, where breaking it changes nothing
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                public int Value { get; set; }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample.Value: Missing documentation. Required blocks: summary");
    }

    [Fact]
    public void Lint_TripleNestedTypeMember_QualifiesByEveryContainingType()
    {
        // arrange — the qualifying loop walks every containing type; two levels cannot tell a loop
        // apart from a single-step lookup
        const string source = """
            namespace T;

            /// <summary>Outer.</summary>
            public class Outer
            {
                /// <summary>Middle.</summary>
                public class Middle
                {
                    /// <summary>Inner.</summary>
                    public class Inner
                    {
                        public string Run(string input) => input;
                    }
                }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors
            .Has(1)
            .At(0)
            .IsEqual(
                "T.Outer.Middle.Inner.Run(string): Missing documentation. Required blocks: summary, param, returns"
            );
    }

    [Fact]
    public void Lint_UndocumentedVoidMethod_DoesNotRequireReturns()
    {
        // arrange — CheckReturns exempts void, so the guidance must not ask for a <returns> block
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                public void Run(string input) { }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample.Run(string): Missing documentation. Required blocks: summary, param");
    }

    [Fact]
    public void Lint_UndocumentedVoidDelegate_DoesNotRequireReturns()
    {
        // arrange — delegates run through the same CheckInvocable path as methods
        const string source = """
            namespace T;

            public delegate void Handler(int value);
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Handler(int): Missing documentation. Required blocks: summary, param");
    }

    [Fact]
    public void Lint_UndocumentedPrimaryConstructorType_RequiresSummaryAndParam()
    {
        // arrange — the primary-ctor parameters are documented on the type, so a type that has one
        // needs <param> too; reported as a single combined message, as CheckInvocable does
        const string source = """
            namespace T;

            public record Sample(int Value);
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample: Missing documentation. Required blocks: summary, param");
    }

    [Fact]
    public void Lint_DocumentedEvent_ReportsNothing()
    {
        // arrange — events were the only member kind covered by negative cases alone
        const string source = """
            using System;

            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Raised when the sample changes.</summary>
                public event Action Changed;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public void Lint_DocumentedEventWithAccessors_ReportsNothing()
    {
        // arrange — the accessor form goes through a different switch arm than the field form
        const string source = """
            using System;

            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Raised when the sample changes.</summary>
                public event Action Changed
                {
                    add { }
                    remove { }
                }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public void Lint_UndocumentedParameterlessPrimaryConstructorType_RequiresSummaryOnly()
    {
        // arrange — an empty parameter list has nothing to document, so the guidance must not ask for
        // <param>; regression for the primary-ctor required-blocks text
        const string source = """
            namespace T;

            public record Sample();
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample: Missing documentation. Required blocks: summary");
    }

    [Fact]
    public void Lint_UndocumentedDestructor_ReportsSummary()
    {
        // arrange — destructors had no switch arm at all and were skipped silently
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                ~Sample() { }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample.~Sample: Missing documentation. Required blocks: summary");
    }

    [Fact]
    public void Lint_DocumentedDestructor_ReportsNothing()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Finalizes the sample.</summary>
                ~Sample() { }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public void Lint_NestedDelegate_QualifiesByContainingType()
    {
        // arrange — every other delegate test declares it at namespace scope, so the parent walk was
        // never exercised for the delegate arm of GetQualifiedName
        const string source = """
            namespace T;

            /// <summary>Outer.</summary>
            public class Outer
            {
                public delegate string Handler(int input);
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors
            .Has(1)
            .At(0)
            .IsEqual("T.Outer.Handler(int): Missing documentation. Required blocks: summary, param, returns");
    }

    [Fact]
    public void Lint_EscapedKeywordParameterMissingDoc_ReportsNameAsWritten()
    {
        // arrange — the @ is stripped when matching against <param name="class">, but the reported
        // name keeps it; only the matching path was covered
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Runs.</summary>
                /// <returns>The output.</returns>
                public string Run(string @class) => @class;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample.Run(string).@class: Missing or empty parameter documentation");
    }

    [Fact]
    public void Lint_UndocumentedOverloads_ReportsDistinguishableNames()
    {
        // arrange — overloads share an identifier, so a name without the parameter signature makes the
        // two errors indistinguishable and the caller cannot tell which overload to document
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                public string Run(string input) => input;

                public string Run(int input) => input.ToString();
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(2);
        errors.At(0).IsEqual("T.Sample.Run(string): Missing documentation. Required blocks: summary, param, returns");
        errors.At(1).IsEqual("T.Sample.Run(int): Missing documentation. Required blocks: summary, param, returns");
    }

    [Fact]
    public void Lint_UndocumentedGenericType_RequiresTypeParam()
    {
        // arrange — a type parameter needs a <typeparam> block, so the guidance must ask for one
        const string source = """
            namespace T;

            public class Sample<T> { }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample<T>: Missing documentation. Required blocks: summary, typeparam");
    }

    [Fact]
    public void Lint_GenericTypeMissingTypeParamDoc_ReportsTypeParameter()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample<T> { }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample<T>.T: Missing or empty type parameter documentation");
    }

    [Fact]
    public void Lint_DocumentedGenericType_ReportsNothing()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            /// <typeparam name="T">The payload type.</typeparam>
            public class Sample<T> { }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public void Lint_GenericMethodMissingTypeParamDoc_ReportsTypeParameter()
    {
        // arrange — methods carry their own type parameters, checked separately from the type's
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Gets.</summary>
                /// <param name="value">The value.</param>
                /// <returns>The value.</returns>
                public T Get<T>(T value) => value;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample.Get<T>(T).T: Missing or empty type parameter documentation");
    }

    [Fact]
    public void Lint_DocumentedGenericMethod_ReportsNothing()
    {
        // arrange
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                /// <summary>Gets.</summary>
                /// <typeparam name="T">The value type.</typeparam>
                /// <param name="value">The value.</param>
                /// <returns>The value.</returns>
                public T Get<T>(T value) => value;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public void Lint_UndocumentedGenericMethod_RequiresTypeParamBeforeParam()
    {
        // arrange — the guidance lists blocks in the order they appear in a doc comment
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            /// <typeparam name="T">The payload type.</typeparam>
            public class Sample<T>
            {
                public TOut Map<TOut>(T value) => default!;
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors
            .Has(1)
            .At(0)
            .IsEqual(
                "T.Sample<T>.Map<TOut>(T): Missing documentation. Required blocks: summary, typeparam, param, returns"
            );
    }

    [Fact]
    public void Lint_UndocumentedGenericPrimaryConstructorType_RequiresTypeParamAndParam()
    {
        // arrange — every generics case is a plain type and every primary-ctor case is non-generic, so
        // the combined shape pins that neither check is dropped and the guidance keeps doc-comment order
        const string source = """
            namespace T;

            public record Sample<T>(T Value);
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample<T>: Missing documentation. Required blocks: summary, typeparam, param");
    }

    [Fact]
    public void Lint_GenericPrimaryConstructorTypeWithSummaryOnly_ReportsTypeParameterAndParameter()
    {
        // arrange — both type-level checks run off the same doc block, so one shadowing the other would
        // otherwise go unnoticed
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public record Sample<T>(T Value);
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(2);
        errors.At(0).IsEqual("T.Sample<T>.T: Missing or empty type parameter documentation");
        errors.At(1).IsEqual("T.Sample<T>.Value: Missing or empty parameter documentation");
    }

    [Fact]
    public void Lint_GenericTypeWithOnlySomeTypeParametersDocumented_ReportsTheUndocumentedOnes()
    {
        // arrange — every other generics case has a single type parameter, so a check that matched any
        // <typeparam> tag regardless of its name would go unnoticed
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            /// <typeparam name="T1">The first type.</typeparam>
            public class Sample<T1, T2> { }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample<T1, T2>.T2: Missing or empty type parameter documentation");
    }

    [Fact]
    public void Lint_OverloadsDifferingOnlyByParameterModifier_ReportsDistinguishableNames()
    {
        // arrange — `ref` / `out` / `in` are part of the overload's identity, so a signature built from
        // parameter types alone renders these two overloads under the same name
        const string source = """
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                public void Run(int input) { }

                public void Run(ref int input) { }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(2);
        errors.At(0).IsEqual("T.Sample.Run(int): Missing documentation. Required blocks: summary, param");
        errors.At(1).IsEqual("T.Sample.Run(ref int): Missing documentation. Required blocks: summary, param");
    }

    [Theory]
    [InlineData("ref int input", "ref int")]
    [InlineData("out int input", "out int")]
    [InlineData("in int input", "in int")]
    [InlineData("ref readonly int input", "ref readonly int")]
    // `params`, `this` and `scoped` are not part of an overload's identity, so they are left out
    [InlineData("params int[] input", "int[]")]
    [InlineData("this int input", "int")]
    public void Lint_ParameterModifiers_AppearInReportedNameOnlyWhenPartOfOverloadIdentity(
        string parameter,
        string signature
    )
    {
        // arrange — the reported name carries exactly the modifiers two overloads can differ by
        var source = $$"""
            namespace T;

            /// <summary>Sample.</summary>
            public class Sample
            {
                public void Run({{parameter}}) { }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors
            .Has(1)
            .At(0)
            .IsEqual($"T.Sample.Run({signature}): Missing documentation. Required blocks: summary, param");
    }

    [Fact]
    public void Lint_ExtensionBlockMember_QualifiesByContainingTypeAndSkipsTheBlock()
    {
        // arrange — an extension block is an unnamed type declaration, so naming it like a type yields
        // `Sample.` and `Sample..Doubled`; only its members are documentable
        const string source = """
            namespace T;

            /// <summary>Holder.</summary>
            public static class Sample
            {
                extension(int value)
                {
                    public int Doubled => value * 2;
                }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.Has(1).At(0).IsEqual("T.Sample.Doubled: Missing documentation. Required blocks: summary");
    }

    [Fact]
    public void Lint_DocumentedExtensionBlock_ReportsNothing()
    {
        // arrange — neither the block nor its receiver parameter is documentable, so a block whose
        // members are documented must report nothing
        const string source = """
            namespace T;

            /// <summary>Holder.</summary>
            public static class Sample
            {
                extension(int value)
                {
                    /// <summary>Adds the other value.</summary>
                    /// <param name="other">The other value.</param>
                    /// <returns>The sum.</returns>
                    public int Add(int other) => value + other;
                }
            }
            """;

        // act
        var errors = Lint(source);

        // assert
        errors.IsEmpty();
    }

    [Fact]
    public void Lint_UndocumentedGenericDelegate_RequiresTypeParam()
    {
        // arrange — delegates take type parameters too and run the same CheckInvocable path
        const string source = """
            namespace T;

            public delegate T Handler<T>(T input);
            """;

        // act
        var errors = Lint(source);

        // assert
        errors
            .Has(1)
            .At(0)
            .IsEqual("T.Handler<T>(T): Missing documentation. Required blocks: summary, typeparam, param, returns");
    }
}
