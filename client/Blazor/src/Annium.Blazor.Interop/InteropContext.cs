using System;
using Annium.Core.DependencyInjection;
using Microsoft.JSInterop;

namespace Annium.Blazor.Interop;

public static class InteropContext
{
    public static IInteropContext Instance { get; private set; } = default!;

    public static void Init(IServiceProvider sp)
    {
        if (Instance != default!)
            throw new InvalidOperationException("Can't init more than once");

        Instance = sp.Resolve<IInteropContext>();
    }
}

internal sealed class InteropContextInstance : IInteropContext
{
    public IJSInProcessRuntime InProcessRuntime { get; }

    public InteropContextInstance(IJSInProcessRuntime inProcessRuntime)
    {
        InProcessRuntime = inProcessRuntime;
    }
}

public interface IInteropContext
{
    IJSInProcessRuntime InProcessRuntime { get; }
}
