using Annium.AspNetCore.Extensions;
using Annium.Infrastructure.Hosting;
using Annium.Logging.Microsoft;
using Annium.XRest.Demo.Server;
using Annium.XRest.Sources.AspNetCore;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServicePack<ServicePack>();
builder.Logging.ConfigureLoggingBridge();
builder.WebHost.UseKestrelDefaults();

var app = builder.Build();

app.UseExceptionMiddleware();
app.UseRouting();
app.UseXRest();
app.UseCorsDefaults();
app.UseRequestLocalization("en", "ru");
app.MapControllers();

await app.RunAsync();
