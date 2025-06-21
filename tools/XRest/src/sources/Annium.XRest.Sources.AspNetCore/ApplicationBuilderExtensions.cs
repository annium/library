using Microsoft.AspNetCore.Builder;

namespace Annium.XRest.Sources.AspNetCore;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseXRest(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<XRestMiddleware>();
    }
}
