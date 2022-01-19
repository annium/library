using System;
using System.Runtime.CompilerServices;
using Microsoft.JSInterop;

namespace Annium.Blazor.Interop.Internal.Extensions;

public static class InteropContextExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Invoke<T>(this IInteropContext ctx, string identifier, params object?[]? args)
        => ctx.InProcessRuntime.Invoke<T>(Call.Name(identifier), args);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvokeVoid(this IInteropContext ctx, string identifier, params object?[]? args)
        => ctx.InProcessRuntime.InvokeVoid(Call.Name(identifier), args);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T UInvoke<T>(this IInteropContext ctx, string identifier)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<T>(Call.Name(identifier));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UInvokeVoid(this IInteropContext ctx, string identifier)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<object>(Call.Name(identifier));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T UInvoke<T1, T>(this IInteropContext ctx, string identifier, T1 x1)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1>, T>(Call.Name(identifier), ValueTuple.Create(x1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UInvokeVoid<T1>(this IInteropContext ctx, string identifier, T1 x1)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1>, object>(Call.Name(identifier), ValueTuple.Create(x1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T UInvoke<T1, T2, T>(this IInteropContext ctx, string identifier, T1 x1, T2 x2)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2>, T>(Call.Name(identifier), ValueTuple.Create(x1, x2));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UInvokeVoid<T1, T2>(this IInteropContext ctx, string identifier, T1 x1, T2 x2)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2>, object>(Call.Name(identifier), ValueTuple.Create(x1, x2));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T UInvoke<T1, T2, T3, T>(this IInteropContext ctx, string identifier, T1 x1, T2 x2, T3 x3)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2, T3>, T>(Call.Name(identifier), ValueTuple.Create(x1, x2, x3));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UInvokeVoid<T1, T2, T3>(this IInteropContext ctx, string identifier, T1 x1, T2 x2, T3 x3)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2, T3>, object>(Call.Name(identifier), ValueTuple.Create(x1, x2, x3));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T UInvoke<T1, T2, T3, T4, T>(this IInteropContext ctx, string identifier, T1 x1, T2 x2, T3 x3, T4 x4)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2, T3, T4>, T>(Call.Name(identifier), ValueTuple.Create(x1, x2, x3, x4));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UInvokeVoid<T1, T2, T3, T4>(this IInteropContext ctx, string identifier, T1 x1, T2 x2, T3 x3, T4 x4)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2, T3, T4>, object>(Call.Name(identifier), ValueTuple.Create(x1, x2, x3, x4));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T UInvoke<T1, T2, T3, T4, T5, T>(this IInteropContext ctx, string identifier, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2, T3, T4, T5>, T>(Call.Name(identifier), ValueTuple.Create(x1, x2, x3, x4, x5));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UInvokeVoid<T1, T2, T3, T4, T5>(this IInteropContext ctx, string identifier, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2, T3, T4, T5>, object>(Call.Name(identifier), ValueTuple.Create(x1, x2, x3, x4, x5));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T UInvoke<T1, T2, T3, T4, T5, T6, T>(this IInteropContext ctx, string identifier, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2, T3, T4, T5, T6>, T>(Call.Name(identifier), ValueTuple.Create(x1, x2, x3, x4, x5, x6));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UInvokeVoid<T1, T2, T3, T4, T5, T6>(this IInteropContext ctx, string identifier, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2, T3, T4, T5, T6>, object>(Call.Name(identifier), ValueTuple.Create(x1, x2, x3, x4, x5, x6));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T UInvoke<T1, T2, T3, T4, T5, T6, T7, T>(this IInteropContext ctx, string identifier, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6, T7 x7)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2, T3, T4, T5, T6, T7>, T>(Call.Name(identifier), ValueTuple.Create(x1, x2, x3, x4, x5, x6, x7));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UInvokeVoid<T1, T2, T3, T4, T5, T6, T7>(this IInteropContext ctx, string identifier, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6, T7 x7)
        => ctx.UnmarshalledRuntime.InvokeUnmarshalled<ValueTuple<T1, T2, T3, T4, T5, T6, T7>, object>(Call.Name(identifier), ValueTuple.Create(x1, x2, x3, x4, x5, x6, x7));
}