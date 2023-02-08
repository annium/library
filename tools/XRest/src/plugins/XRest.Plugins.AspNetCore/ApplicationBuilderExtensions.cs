using Microsoft.AspNetCore.Builder;
using XRest.Plugins.AspNetCore;

// ReSharper disable once CheckNamespace

namespace Annium.Core.DependencyInjection;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseXRest(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<XRestMiddleware>();
    }
}