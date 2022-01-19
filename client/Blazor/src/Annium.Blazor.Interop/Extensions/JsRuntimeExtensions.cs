using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Annium.Blazor.Interop;

public static class JsRuntimeExtensions
{
    public static async Task<IJSUnmarshalledObjectReference> ImportAsync(this IJSRuntime js, string path)
    {
        return await js.InvokeAsync<IJSUnmarshalledObjectReference>("import", path);
    }
}