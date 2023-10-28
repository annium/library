using System.Runtime.CompilerServices;
using Microsoft.JSInterop;

namespace Annium.Blazor.Interop.Internal.Extensions;

public static class InteropContextExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Call<T>(this IInteropContext ctx, string identifier, params object?[]? args) =>
        ctx.InProcessRuntime.Invoke<T>(Helper.Call(identifier), args);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Call(this IInteropContext ctx, string identifier, params object?[]? args) =>
        ctx.InProcessRuntime.InvokeVoid(Helper.Call(identifier), args);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Apply<T>(this IInteropContext ctx, string identifier, object?[] args) =>
        ctx.InProcessRuntime.Invoke<T>(Helper.Call(identifier), args);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Apply(this IInteropContext ctx, string identifier, object?[] args) =>
        ctx.InProcessRuntime.InvokeVoid(Helper.Call(identifier), args);
}
