using System;
using System.Collections.Generic;

namespace Annium.Blazor.Core.Tools
{
    public interface IClassBuilder<T>
    {
        IClassBuilder<T> Clone();

        string Build(T data);

        IClassBuilder<T> With(string? className);

        IClassBuilder<T> With(Func<bool> predicate, string? className);

        IClassBuilder<T> With(Func<T, bool> predicate, string? className);

        IClassBuilder<T> With(Func<string?> fetch);

        IClassBuilder<T> With(Func<T, string?> fetch);

        IClassBuilder<T> With(Func<bool> predicate, Func<string?> fetch);

        IClassBuilder<T> With(Func<T, bool> predicate, Func<string?> fetch);

        IClassBuilder<T> With(Func<bool> predicate, Func<T, string?> fetch);

        IClassBuilder<T> With(Func<T, bool> predicate, Func<T, string?> fetch);

        IClassBuilder<T> With<TK>(Func<T, TK> getKey, IDictionary<TK, string?> dictionary);
    }

    public interface IClassBuilder
    {
        IClassBuilder Clone();

        string Build();

        IClassBuilder With(string? className);

        IClassBuilder With(Func<bool> predicate, string? className);

        IClassBuilder With(Func<string?> fetch);

        IClassBuilder With(Func<bool> predicate, Func<string?> fetch);

        IClassBuilder With<TK>(TK key, IDictionary<TK, string?> dictionary);
    }
}