using System;
using System.IO;
using System.Threading.Tasks;
using Annium.DocLint.Commands;
using Annium.DocLint.Internal.Services;
using Annium.Testing;
using Xunit;

namespace Annium.DocLint.Tests;

// LintCommand is the `just docs-lint` gate every other Annium repo runs in CI: what matters here is
// that it fails loudly rather than reporting success for work it never did.
public class LintCommandTests
{
    private const string Documented = """
        namespace T;

        /// <summary>A documented type.</summary>
        public class Sample
        {
            /// <summary>Runs.</summary>
            /// <param name="input">The input.</param>
            /// <returns>The output.</returns>
            public string Run(string input) => input;
        }
        """;

    private const string Undocumented = """
        namespace T;

        public class Sample
        {
            public string Run(string input) => input;
        }
        """;

    private readonly LintCommand _command = new(new LintService());

    [Fact]
    public async Task HandleAsync_WorkingDirectoryMissing_Throws()
    {
        // arrange — a mistyped -w must not lint nothing and exit 0
        var missing = Path.Combine(Path.GetTempPath(), $"doclint-missing-{Guid.NewGuid():N}");
        var cfg = new LintCommandConfiguration { WorkingDirectory = missing, Include = ["**/*.cs"] };

        // act
        var exception = await Wrap.It(async () =>
                await _command.HandleAsync(cfg, TestContext.Current.CancellationToken)
            )
            .ThrowsAsync<DirectoryNotFoundException>();

        // assert
        exception.Message.IsEqual($"Working directory {Path.GetFullPath(missing)} does not exist");
    }

    [Fact]
    public async Task HandleAsync_NoFilesMatched_Throws()
    {
        // arrange — a glob matching nothing is a broken invocation, not a clean run
        using var sources = TempSources.Create(("Sample.cs", Documented));
        var cfg = new LintCommandConfiguration { WorkingDirectory = sources.Root, Include = ["**/*.nomatch"] };

        // act
        var exception = await Wrap.It(async () =>
                await _command.HandleAsync(cfg, TestContext.Current.CancellationToken)
            )
            .ThrowsAsync<InvalidOperationException>();

        // assert
        exception.Message.IsContaining("No files matched");
    }

    [Fact]
    public async Task HandleAsync_AllFilesDocumented_Completes()
    {
        // arrange
        using var sources = TempSources.Create(("Sample.cs", Documented), ("nested/Other.cs", Documented));
        var cfg = new LintCommandConfiguration { WorkingDirectory = sources.Root, Include = ["**/*.cs"] };

        // act
        var exception = await Record.ExceptionAsync(() =>
            _command.HandleAsync(cfg, TestContext.Current.CancellationToken)
        );

        // assert
        exception.IsNull();
    }

    [Fact]
    public async Task HandleAsync_FileWithErrors_Throws()
    {
        // arrange
        using var sources = TempSources.Create(("Sample.cs", Documented), ("Broken.cs", Undocumented));
        var cfg = new LintCommandConfiguration { WorkingDirectory = sources.Root, Include = ["**/*.cs"] };

        // act
        var exception = await Wrap.It(async () =>
                await _command.HandleAsync(cfg, TestContext.Current.CancellationToken)
            )
            .ThrowsAsync<Exception>();

        // assert
        exception.Message.IsEqual("linting failed");
    }

    [Fact]
    public async Task HandleAsync_PartialTypeDocumentedInAnotherFile_Completes()
    {
        // arrange — a partial type is documented once, on whichever declaration its author chose, so
        // its other files must not be told the type is undocumented
        const string documentedPart = """
            namespace T;

            /// <summary>A documented type.</summary>
            public partial class Split
            {
                /// <summary>Runs.</summary>
                public void A() { }
            }
            """;
        const string otherPart = """
            namespace T;

            public partial class Split
            {
                /// <summary>Runs.</summary>
                public void B() { }
            }
            """;
        using var sources = TempSources.Create(("Split.A.cs", documentedPart), ("Split.B.cs", otherPart));
        var cfg = new LintCommandConfiguration { WorkingDirectory = sources.Root, Include = ["**/*.cs"] };

        // act
        var exception = await Record.ExceptionAsync(() =>
            _command.HandleAsync(cfg, TestContext.Current.CancellationToken)
        );

        // assert
        exception.IsNull();
    }

    [Fact]
    public async Task HandleAsync_PartialTypeDocumentedNowhere_Throws()
    {
        // arrange — suppression must hold only when some file actually documents the type
        const string source = """
            namespace T;

            public partial class Split
            {
                /// <summary>Runs.</summary>
                public void A() { }
            }
            """;
        using var sources = TempSources.Create(("Split.A.cs", source), ("Split.B.cs", source.Replace("A()", "B()")));
        var cfg = new LintCommandConfiguration { WorkingDirectory = sources.Root, Include = ["**/*.cs"] };

        // act
        var exception = await Wrap.It(async () =>
                await _command.HandleAsync(cfg, TestContext.Current.CancellationToken)
            )
            .ThrowsAsync<Exception>();

        // assert
        exception.Message.IsEqual("linting failed");
    }

    [Fact]
    public async Task HandleAsync_PartialTypeWithPrimaryConstructorDocumentedElsewhere_Throws()
    {
        // arrange — the sibling's summary documents the type but cannot document these parameters, so
        // the declaration that owns them must still be reported
        const string withParameters = """
            namespace T;

            public partial class Split(int value)
            {
                /// <summary>Runs.</summary>
                public void A() { }
            }
            """;
        const string documented = """
            namespace T;

            /// <summary>A documented type.</summary>
            public partial class Split
            {
                /// <summary>Runs.</summary>
                public void B() { }
            }
            """;
        using var sources = TempSources.Create(("Split.A.cs", withParameters), ("Split.B.cs", documented));
        var cfg = new LintCommandConfiguration { WorkingDirectory = sources.Root, Include = ["**/*.cs"] };

        // act
        var exception = await Wrap.It(async () =>
                await _command.HandleAsync(cfg, TestContext.Current.CancellationToken)
            )
            .ThrowsAsync<Exception>();

        // assert
        exception.Message.IsEqual("linting failed");
    }

    [Fact]
    public async Task HandleAsync_FilePartialTypesOfTheSameName_DoNotShareDocumentation()
    {
        // arrange — `file` types are per-file, so these are two unrelated types; documenting one must
        // not report the other's file as clean
        const string documented = """
            namespace T;

            /// <summary>A documented file-local type.</summary>
            file partial class Sample
            {
                /// <summary>Runs.</summary>
                public void A() { }
            }
            """;
        const string undocumented = """
            namespace T;

            file partial class Sample
            {
                /// <summary>Runs.</summary>
                public void B() { }
            }
            """;
        using var sources = TempSources.Create(("One.cs", documented), ("Two.cs", undocumented));
        var cfg = new LintCommandConfiguration { WorkingDirectory = sources.Root, Include = ["**/*.cs"] };

        // act
        var exception = await Wrap.It(async () =>
                await _command.HandleAsync(cfg, TestContext.Current.CancellationToken)
            )
            .ThrowsAsync<Exception>();

        // assert
        exception.Message.IsEqual("linting failed");
    }

    [Fact]
    public async Task HandleAsync_PartialTypesDifferingByArity_DoNotShareDocumentation()
    {
        // arrange — an arity-blind key let a documented `Split<T>` hide an undocumented `Split<T, U>`,
        // shipping an undocumented public type through the gate
        const string documented = """
            namespace T;

            /// <summary>A documented type.</summary>
            /// <typeparam name="T1">The first type.</typeparam>
            public partial class Split<T1>
            {
                /// <summary>Runs.</summary>
                public void Run() { }
            }
            """;
        const string undocumented = """
            namespace T;

            public partial class Split<T1, T2>
            {
                /// <summary>Runs.</summary>
                public void Run() { }
            }
            """;
        using var sources = TempSources.Create(("One.cs", documented), ("Two.cs", undocumented));
        var cfg = new LintCommandConfiguration { WorkingDirectory = sources.Root, Include = ["**/*.cs"] };

        // act
        var exception = await Wrap.It(async () =>
                await _command.HandleAsync(cfg, TestContext.Current.CancellationToken)
            )
            .ThrowsAsync<Exception>();

        // assert
        exception.Message.IsEqual("linting failed");
    }

    [Fact]
    public async Task HandleAsync_SameNamedPartialTypesInDifferentNamespaces_DoNotShareDocumentation()
    {
        // arrange — the suppression key carries the namespace, so documenting one type must not
        // silence a same-named type elsewhere
        const string documented = """
            namespace A;

            /// <summary>A documented type.</summary>
            public partial class Split
            {
                /// <summary>Runs.</summary>
                public void Run() { }
            }
            """;
        const string undocumented = """
            namespace B;

            public partial class Split
            {
                /// <summary>Runs.</summary>
                public void Run() { }
            }
            """;
        using var sources = TempSources.Create(("A.cs", documented), ("B.cs", undocumented));
        var cfg = new LintCommandConfiguration { WorkingDirectory = sources.Root, Include = ["**/*.cs"] };

        // act
        var exception = await Wrap.It(async () =>
                await _command.HandleAsync(cfg, TestContext.Current.CancellationToken)
            )
            .ThrowsAsync<Exception>();

        // assert
        exception.Message.IsEqual("linting failed");
    }

    [Fact]
    public async Task HandleAsync_ExcludedFileWithErrors_Completes()
    {
        // arrange — exclude must win over include, or the gate lints files it was told to skip
        using var sources = TempSources.Create(("Sample.cs", Documented), ("generated/Broken.cs", Undocumented));
        var cfg = new LintCommandConfiguration
        {
            WorkingDirectory = sources.Root,
            Include = ["**/*.cs"],
            Exclude = ["generated/**/*.cs"],
        };

        // act
        var exception = await Record.ExceptionAsync(() =>
            _command.HandleAsync(cfg, TestContext.Current.CancellationToken)
        );

        // assert
        exception.IsNull();
    }
}
