using System.Collections.Generic;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Annium.XRest.Clients.Csharp.Components.Processors;
using Xunit;

namespace Annium.XRest.Clients.Csharp.Tests.Components.Processors;

public class RefProcessorTests
{
    [Theory]
    [InlineData(BaseType.Object, "object")]
    [InlineData(BaseType.Bool, "bool")]
    [InlineData(BaseType.String, "string")]
    [InlineData(BaseType.Int, "int")]
    [InlineData(BaseType.Long, "long")]
    [InlineData(BaseType.Decimal, "decimal")]
    public void Process_PrimitiveBaseType_EmitsKeyword(string baseType, string expected)
    {
        // assert
        Process(new BaseTypeRef(baseType)).Is(expected);
    }

    [Theory]
    // must mirror Annium.Net.Types' MapperConfigExtensions: LocalDateTime ↔ dateTime, LocalTime ↔ time.
    // Regression: dateTime used to emit LocalTime, silently dropping the date component
    [InlineData(BaseType.DateTime, "LocalDateTime")]
    [InlineData(BaseType.DateTimeOffset, "Instant")]
    [InlineData(BaseType.Date, "DateOnly")]
    [InlineData(BaseType.Time, "TimeOnly")]
    [InlineData(BaseType.TimeSpan, "Duration")]
    [InlineData(BaseType.YearMonth, "YearMonth")]
    [InlineData(BaseType.Guid, "Guid")]
    public void Process_TemporalBaseType_MatchesMapperConfig(string baseType, string expected)
    {
        // assert
        Process(new BaseTypeRef(baseType)).Is(expected);
    }

    [Fact]
    public void Process_Void_EmitsTheKeyword()
    {
        // assert — `System.Void` cannot be written in C#, so emitting the CLR name produced code that
        // did not compile (CS0673)
        Process(new BaseTypeRef(BaseType.Void)).Is("void");
    }

    [Fact]
    public void Process_Nullable_AppendsQuestionMark()
    {
        // assert
        Process(new NullableRef(new BaseTypeRef(BaseType.Int))).Is("int?");
    }

    [Fact]
    public void Process_Array_AppendsBrackets()
    {
        // assert
        Process(new ArrayRef(new BaseTypeRef(BaseType.String))).Is("string[]");
    }

    [Fact]
    public void Process_Record_EmitsDictionary()
    {
        // assert
        Process(new RecordRef(new BaseTypeRef(BaseType.String), new BaseTypeRef(BaseType.Int)))
            .Is("Dictionary<string, int>");
    }

    [Fact]
    public void Process_GenericParameter_EmitsName()
    {
        // assert
        Process(new GenericParameterRef("T")).Is("T");
    }

    [Fact]
    public void Process_Promise_WithValue_EmitsGenericTask()
    {
        // assert
        Process(new PromiseRef(new BaseTypeRef(BaseType.String))).Is("Task<string>");
    }

    [Fact]
    public void Process_Promise_WithoutValue_EmitsTask()
    {
        // assert
        Process(new PromiseRef(null)).Is("Task");
    }

    [Fact]
    public void Process_StructWithoutArgs_EmitsName()
    {
        // assert
        Process(new StructRef("Demo.Models", "User")).Is("User");
    }

    [Fact]
    public void Process_GenericStruct_EmitsArguments()
    {
        // assert
        Process(new StructRef("Demo.Models", "Page", new BaseTypeRef(BaseType.Int), new BaseTypeRef(BaseType.String)))
            .Is("Page<int, string>");
    }

    [Fact]
    public void Process_Enum_EmitsNameAndRegistersNamespace()
    {
        // arrange
        var ctx = CreateContext();

        // act
        var result = RefProcessor.Process(new EnumRef("Demo.Models", "Status"), ctx);

        // assert — an enum this API does not own is imported from where it actually lives
        result.Is("Status");
        ctx.Usages.Has(1).At(0).ToString().Is("Demo.Models");
    }

    [Fact]
    public void Process_EnumThisApiOwns_ImportsItFromTheGeneratedModelsNamespace()
    {
        // arrange — the model list is what tells the two apart, and every other case here passes an
        // empty one, so this branch decides where half the generated usings point
        var ctx = new ProcessingContext(
            "Client.Models".ToNamespace(),
            [new EnumModel("Demo.Models".ToNamespace(), "Status", new Dictionary<string, long>())]
        );

        // act
        var result = RefProcessor.Process(new EnumRef("Demo.Models", "Status"), ctx);

        // assert
        result.Is("Status");
        ctx.Usages.Has(1).At(0).ToString().Is("Client.Models.Demo.Models");
    }

    [Fact]
    public void Process_StructThisApiOwns_ImportsItFromTheGeneratedModelsNamespace()
    {
        // arrange
        var ctx = new ProcessingContext(
            "Client.Models".ToNamespace(),
            [new StructModel("Demo.Models".ToNamespace(), false, "Page")]
        );

        // act
        var result = RefProcessor.Process(new StructRef("Demo.Models", "Page"), ctx);

        // assert
        result.Is("Page");
        ctx.Usages.Has(1).At(0).ToString().Is("Client.Models.Demo.Models");
    }

    [Fact]
    public void Process_StructThisApiDoesNotOwn_ImportsItFromItsOwnNamespace()
    {
        // arrange
        var ctx = CreateContext();

        // act
        RefProcessor.Process(new StructRef("Other.Models", "Page"), ctx);

        // assert
        ctx.Usages.Has(1).At(0).ToString().Is("Other.Models");
    }

    [Fact]
    public void Process_BaseTypeNeedingImport_RegistersNamespace()
    {
        // arrange
        var ctx = CreateContext();

        // act
        RefProcessor.Process(new BaseTypeRef(BaseType.DateTime), ctx);

        // assert — NodaTime must be imported for LocalDateTime to compile; asserting merely that
        // something was registered passes even when the wrong namespace is
        ctx.Usages.Has(1).At(0).ToString().Is("NodaTime");
    }

    [Fact]
    public void Process_ModelNamedLikeAContainer_IsWrittenInFull()
    {
        // arrange — regression: a model named `Root` resolved to the generated `Root` container, which
        // is declared in the client file's own namespace and therefore wins over the `using`. Nothing
        // failed to compile; the method simply returned the wrong type
        var ctx = new ProcessingContext(
            "Client.Models".ToNamespace(),
            [new StructModel("Demo.Models".ToNamespace(), false, "Root")]
        )
        {
            ReservedNames = ["Root"],
        };

        // act
        var result = RefProcessor.Process(new StructRef("Demo.Models", "Root"), ctx);

        // assert
        result.Is("global::Client.Models.Demo.Models.Root");
        ctx.Usages.IsEmpty();
    }

    [Fact]
    public void Process_ModelNameSharedAcrossNamespaces_IsWrittenInFull()
    {
        // arrange — two `Page<T>` from different namespaces used to be written short, and both usings
        // landed in the same file — CS0104
        var ctx = new ProcessingContext(
            "Client.Models".ToNamespace(),
            [
                new StructModel("Demo.Foo".ToNamespace(), false, "Page"),
                new StructModel("Demo.Bar".ToNamespace(), false, "Page"),
            ]
        );

        // act
        var result = RefProcessor.Process(new StructRef("Demo.Foo", "Page"), ctx);

        // assert
        result.Is("global::Client.Models.Demo.Foo.Page");
    }

    [Fact]
    public void Process_EnumNamedLikeAContainer_IsWrittenInFull()
    {
        // arrange — enums take the same route as the other models, and the same collision applies
        var ctx = new ProcessingContext(
            "Client.Models".ToNamespace(),
            [new EnumModel("Demo.Models".ToNamespace(), "Root", new Dictionary<string, long>())]
        )
        {
            ReservedNames = ["Root"],
        };

        // act
        var result = RefProcessor.Process(new EnumRef("Demo.Models", "Root"), ctx);

        // assert
        result.Is("global::Client.Models.Demo.Models.Root");
        ctx.Usages.IsEmpty();
    }

    private static string Process(IRef reference) => RefProcessor.Process(reference, CreateContext());

    private static ProcessingContext CreateContext() => new("Demo.Models".ToNamespace(), []);
}
