using Microsoft.AspNetCore.Builder;
using XRest.AspNetCore;

namespace Annium.Core.DependencyInjection
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseXRest(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<XRestMiddleware>();
        }
    }
}