using Annium.Data.Operations;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Refs;

namespace XRest.Clients.Csharp.Components.Processors;

internal static class ResponseSpecialProcessor
{
    private const string KindResult = "result";
    private const string KindDataResult = "data-result";

    public static (string, IRef?)? ResolveResponseKindAndInnerType(IRef response, ProcessingContext ctx)
    {
        if (response is not IGenericModelRef modelRef)
            return null;

        if (modelRef.IsFor(typeof(IResult)))
        {
            ctx.UseNamespace(typeof(IResult));

            return (KindResult, null);
        }

        if (modelRef.IsFor(typeof(IResult<>)))
        {
            ctx.UseNamespace(typeof(IResult<>));

            return (KindResult, modelRef.Args[0]);
        }

        return null;
    }
}