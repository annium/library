using System;
using Annium.Core.DependencyInjection;
using Microsoft.JSInterop;

namespace Annium.Blazor.Interop;

/// <summary>
/// Provides static access to the interop context instance for JavaScript interop operations.
/// </summary>
public static class InteropContext
{
    /// <summary>
    /// Gets the singleton instance of the interop context.
    /// </summary>
    public static IInteropContext Instance { get; private set; } = null!;

    /// <summary>
    /// Initializes the interop context with the specified service provider.
    /// </summary>
    /// <param name="sp">The service provider to resolve the interop context from.</param>
    public static void Init(IServiceProvider sp)
    {
        if (Instance != null!)
            throw new InvalidOperationException("Can't init more than once");

        Instance = sp.Resolve<IInteropContext>();
    }
}

/// <summary>
/// Internal implementation of the interop context.
/// </summary>
internal sealed class InteropContextInstance : IInteropContext
{
    /// <summary>
    /// Gets the in-process JavaScript runtime for synchronous JavaScript interop calls.
    /// </summary>
    public IJSInProcessRuntime InProcessRuntime { get; }

    /// <summary>
    /// Initializes a new instance of the InteropContextInstance class.
    /// </summary>
    /// <param name="inProcessRuntime">The in-process JavaScript runtime.</param>
    public InteropContextInstance(IJSInProcessRuntime inProcessRuntime)
    {
        InProcessRuntime = inProcessRuntime;
    }
}

/// <summary>
/// Defines the contract for JavaScript interop context operations.
/// </summary>
public interface IInteropContext
{
    /// <summary>
    /// Gets the in-process JavaScript runtime for synchronous JavaScript interop calls.
    /// </summary>
    IJSInProcessRuntime InProcessRuntime { get; }
}
