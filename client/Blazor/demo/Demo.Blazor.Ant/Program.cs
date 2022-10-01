using Annium.Blazor.Core.Extensions;
using Demo.Blazor.Ant;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.StartAt<App>();
builder.UseServicePack<ServicePack>();
var app = builder.Build();
await app.RunAsync();