using System.Runtime.CompilerServices;
using Annium.Blazor.Interop.Extensions;

namespace Annium.Blazor.Interop.Internal.Extensions;

/// <summary>
/// Provides extension methods for IInteropContext to simplify JavaScript interop calls.
/// </summary>
public static class InteropContextExtensions
{
    /// <summary>
    /// Calls a JavaScript function and returns the result of type T.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="ctx">The interop context.</param>
    /// <param name="identifier">The JavaScript function identifier.</param>
    /// <param name="args">Optional arguments to pass to the function.</param>
    /// <returns>The result of the JavaScript function call.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CallHelper<T>(this IInteropContext ctx, string identifier, params object?[]? args) =>
        ctx.Invoke<T>(Helper.Call(identifier), args);

    /// <summary>
    /// Calls a JavaScript function without expecting a return value.
    /// </summary>
    /// <param name="ctx">The interop context.</param>
    /// <param name="identifier">The JavaScript function identifier.</param>
    /// <param name="args">Optional arguments to pass to the function.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CallHelper(this IInteropContext ctx, string identifier, params object?[]? args) =>
        ctx.Invoke(Helper.Call(identifier), args);
}
