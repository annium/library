using System;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Api;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();
        services.AddControllers().AddDefaultJsonOptions();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseExceptionMiddleware();
        app.UseRouting();
        app.UseCors(builder =>
            builder
                .SetIsOriginAllowed(o => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetPreflightMaxAge(TimeSpan.FromDays(7))
        );
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
