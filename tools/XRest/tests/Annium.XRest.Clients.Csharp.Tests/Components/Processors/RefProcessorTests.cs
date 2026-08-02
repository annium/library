using Annium.Net.Types.Extensions;
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
    public void Process_Void_EmitsVoid()
    {
        // assert
        Process(new BaseTypeRef(BaseType.Void)).Is("Void");
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

        // assert
        result.Is("Status");
        ctx.Usages.IsNotEmpty();
    }

    [Fact]
    public void Process_BaseTypeNeedingImport_RegistersNamespace()
    {
        // arrange
        var ctx = CreateContext();

        // act
        RefProcessor.Process(new BaseTypeRef(BaseType.DateTime), ctx);

        // assert — NodaTime must be imported for LocalDateTime to compile
        ctx.Usages.IsNotEmpty();
    }

    private static string Process(IRef reference) => RefProcessor.Process(reference, CreateContext());

    private static ProcessingContext CreateContext() => new("Demo.Models".ToNamespace(), []);
}
