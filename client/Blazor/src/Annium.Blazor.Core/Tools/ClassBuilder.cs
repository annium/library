using System;
using System.Collections.Generic;
using Annium.Blazor.Core.Internal.Tools;

namespace Annium.Blazor.Core.Tools;

public static class ClassBuilder<T>
{
    public static IClassBuilder<T> With(string? className) => new ClassBuilderInstance<T>().With(className);

    public static IClassBuilder<T> With(Func<bool> predicate, string? className) =>
        new ClassBuilderInstance<T>().With(predicate, className);

    public static IClassBuilder<T> With(Func<T, bool> predicate, string? className) =>
        new ClassBuilderInstance<T>().With(predicate, className);

    public static IClassBuilder<T> With(Func<string?> fetch) => new ClassBuilderInstance<T>().With(fetch);

    public static IClassBuilder<T> With(Func<T, string?> fetch) => new ClassBuilderInstance<T>().With(fetch);

    public static IClassBuilder<T> With(Func<bool> predicate, Func<string?> fetch) =>
        new ClassBuilderInstance<T>().With(predicate, fetch);

    public static IClassBuilder<T> With(Func<T, bool> predicate, Func<string?> fetch) =>
        new ClassBuilderInstance<T>().With(predicate, fetch);

    public static IClassBuilder<T> With(Func<bool> predicate, Func<T, string?> fetch) =>
        new ClassBuilderInstance<T>().With(predicate, fetch);

    public static IClassBuilder<T> With(Func<T, bool> predicate, Func<T, string?> fetch) =>
        new ClassBuilderInstance<T>().With(predicate, fetch);

    public static IClassBuilder<T> With<TK>(Func<T, TK> getKey, IDictionary<TK, string?> dictionary) =>
        new ClassBuilderInstance<T>().With(getKey, dictionary);
}

public static class ClassBuilder
{
    public static IClassBuilder With(string? className) => new ClassBuilderInstance().With(className);

    public static IClassBuilder With(Func<bool> predicate, string? className) =>
        new ClassBuilderInstance().With(className);

    public static IClassBuilder With(Func<string?> fetch) => new ClassBuilderInstance().With(fetch);

    public static IClassBuilder With(Func<bool> predicate, Func<string?> fetch) =>
        new ClassBuilderInstance().With(predicate, fetch);

    public static IClassBuilder With<TK>(TK key, IDictionary<TK, string?> dictionary) =>
        new ClassBuilderInstance().With(key, dictionary);
}
