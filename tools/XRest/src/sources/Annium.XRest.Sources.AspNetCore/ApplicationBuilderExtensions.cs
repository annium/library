using Annium.XRest.Sources.AspNetCore;
using Microsoft.AspNetCore.Builder;

// ReSharper disable once CheckNamespace

namespace Annium.Core.DependencyInjection;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseXRest(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<XRestMiddleware>();
    }
}
