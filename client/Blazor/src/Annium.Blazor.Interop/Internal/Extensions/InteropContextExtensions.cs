using System;
using System.Runtime.CompilerServices;
using Microsoft.JSInterop;

namespace Annium.Blazor.Interop.Internal.Extensions;

public static class InteropContextExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Call<T>(this IInteropContext ctx, string identifier, params object?[]? args)
        => ctx.InProcessRuntime.Invoke<T>(Helper.Call(identifier), args);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Call(this IInteropContext ctx, string identifier, params object?[]? args)
        => ctx.InProcessRuntime.InvokeVoid(Helper.Call(identifier), args);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Apply<T>(this IInteropContext ctx, string identifier, object?[] args)
        => ctx.InProcessRuntime.Invoke<T>(Helper.Call(identifier), args);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Apply(this IInteropContext ctx, string identifier, object?[] args)
        => ctx.InProcessRuntime.InvokeVoid(Helper.Call(identifier), args);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Invoke<T>(this IInteropContext ctx, string identifier)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<T>(Helper.Call(identifier));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Invoke<T1, T>(this IInteropContext ctx, string identifier, T1 x1)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<T1, T>(Helper.Call(identifier), x1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Invoke<T1>(this IInteropContext ctx, string identifier, T1 x1)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<T1, object>(Helper.Call(identifier), x1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Invoke<T1, T2, T>(this IInteropContext ctx, string identifier, T1 x1, T2 x2)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2>, T>(Helper.Call(identifier), ValueTuple.Create(x1, x2));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Invoke<T1, T2>(this IInteropContext ctx, string identifier, T1 x1, T2 x2)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2>, object>(Helper.Call(identifier), ValueTuple.Create(x1, x2));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Invoke<T1, T2, T3>(this IInteropContext ctx, string identifier, T1 x1, T2 x2, T3 x3)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2, T3>, object>(Helper.Call(identifier), ValueTuple.Create(x1, x2, x3));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Invoke<T1, T2, T3, T4, T5>(this IInteropContext ctx, string identifier, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2, T3, T4, T5>, object>(Helper.Call(identifier), ValueTuple.Create(x1, x2, x3, x4, x5));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Invoke<T1, T2, T3, T4, T5, T6, T7>(this IInteropContext ctx, string identifier, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6, T7 x7)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2, T3, T4, T5, T6, T7>, object>(Helper.Call(identifier), ValueTuple.Create(x1, x2, x3, x4, x5, x6, x7));
}