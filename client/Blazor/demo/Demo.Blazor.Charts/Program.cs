using Annium.Blazor.Core.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Demo.Blazor.Charts;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.StartAt<App>();
builder.UseServicePack<ServicePack>();
var app = builder.Build();
await app.RunAsync();