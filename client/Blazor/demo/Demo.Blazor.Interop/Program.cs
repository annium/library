using Annium.Blazor.Core;
using Demo.Blazor.Interop;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.StartAt<App>();
builder.UseServicePack<ServicePack>();
var app = builder.Build();
await app.RunAsync();