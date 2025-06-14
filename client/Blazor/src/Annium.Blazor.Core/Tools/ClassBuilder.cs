using System;
using System.Collections.Generic;
using Annium.Blazor.Core.Internal.Tools;

namespace Annium.Blazor.Core.Tools;

/// <summary>
/// Generic static factory class for creating CSS class builders with typed data support.
/// </summary>
/// <typeparam name="T">The type of data used for building CSS classes.</typeparam>
public static class ClassBuilder<T>
{
    /// <summary>
    /// Creates a new class builder instance with the specified CSS class name.
    /// </summary>
    /// <param name="className">The CSS class name to add.</param>
    /// <returns>A new class builder instance.</returns>
    public static IClassBuilder<T> With(string? className) => new ClassBuilderInstance<T>().With(className);

    /// <summary>
    /// Creates a new class builder instance with a conditional CSS class name.
    /// </summary>
    /// <param name="predicate">The condition that determines if the class should be applied.</param>
    /// <param name="className">The CSS class name to add when the predicate is true.</param>
    /// <returns>A new class builder instance.</returns>
    public static IClassBuilder<T> With(Func<bool> predicate, string? className) =>
        new ClassBuilderInstance<T>().With(predicate, className);

    /// <summary>
    /// Creates a new class builder instance with a data-dependent conditional CSS class name.
    /// </summary>
    /// <param name="predicate">The condition based on data that determines if the class should be applied.</param>
    /// <param name="className">The CSS class name to add when the predicate is true.</param>
    /// <returns>A new class builder instance.</returns>
    public static IClassBuilder<T> With(Func<T, bool> predicate, string? className) =>
        new ClassBuilderInstance<T>().With(predicate, className);

    /// <summary>
    /// Creates a new class builder instance with a dynamic CSS class name fetcher.
    /// </summary>
    /// <param name="fetch">The function that returns the CSS class name.</param>
    /// <returns>A new class builder instance.</returns>
    public static IClassBuilder<T> With(Func<string?> fetch) => new ClassBuilderInstance<T>().With(fetch);

    /// <summary>
    /// Creates a new class builder instance with a data-dependent dynamic CSS class name fetcher.
    /// </summary>
    /// <param name="fetch">The function that takes data and returns the CSS class name.</param>
    /// <returns>A new class builder instance.</returns>
    public static IClassBuilder<T> With(Func<T, string?> fetch) => new ClassBuilderInstance<T>().With(fetch);

    /// <summary>
    /// Creates a new class builder instance with a conditional dynamic CSS class name fetcher.
    /// </summary>
    /// <param name="predicate">The condition that determines if the class should be fetched.</param>
    /// <param name="fetch">The function that returns the CSS class name when the predicate is true.</param>
    /// <returns>A new class builder instance.</returns>
    public static IClassBuilder<T> With(Func<bool> predicate, Func<string?> fetch) =>
        new ClassBuilderInstance<T>().With(predicate, fetch);

    /// <summary>
    /// Creates a new class builder instance with a data-dependent conditional dynamic CSS class name fetcher.
    /// </summary>
    /// <param name="predicate">The condition based on data that determines if the class should be fetched.</param>
    /// <param name="fetch">The function that returns the CSS class name when the predicate is true.</param>
    /// <returns>A new class builder instance.</returns>
    public static IClassBuilder<T> With(Func<T, bool> predicate, Func<string?> fetch) =>
        new ClassBuilderInstance<T>().With(predicate, fetch);

    /// <summary>
    /// Creates a new class builder instance with a conditional data-dependent dynamic CSS class name fetcher.
    /// </summary>
    /// <param name="predicate">The condition that determines if the class should be fetched.</param>
    /// <param name="fetch">The function that takes data and returns the CSS class name when the predicate is true.</param>
    /// <returns>A new class builder instance.</returns>
    public static IClassBuilder<T> With(Func<bool> predicate, Func<T, string?> fetch) =>
        new ClassBuilderInstance<T>().With(predicate, fetch);

    /// <summary>
    /// Creates a new class builder instance with a data-dependent conditional and data-dependent dynamic CSS class name fetcher.
    /// </summary>
    /// <param name="predicate">The condition based on data that determines if the class should be fetched.</param>
    /// <param name="fetch">The function that takes data and returns the CSS class name when the predicate is true.</param>
    /// <returns>A new class builder instance.</returns>
    public static IClassBuilder<T> With(Func<T, bool> predicate, Func<T, string?> fetch) =>
        new ClassBuilderInstance<T>().With(predicate, fetch);

    /// <summary>
    /// Creates a new class builder instance with dictionary-based CSS class lookup.
    /// </summary>
    /// <typeparam name="TK">The type of the dictionary key.</typeparam>
    /// <param name="getKey">The function that extracts the key from the data.</param>
    /// <param name="dictionary">The dictionary mapping keys to CSS class names.</param>
    /// <returns>A new class builder instance.</returns>
    public static IClassBuilder<T> With<TK>(Func<T, TK> getKey, IDictionary<TK, string?> dictionary) =>
        new ClassBuilderInstance<T>().With(getKey, dictionary);
}

/// <summary>
/// Static factory class for creating CSS class builders without typed data support.
/// </summary>
public static class ClassBuilder
{
    /// <summary>
    /// Creates a new class builder instance with the specified CSS class name.
    /// </summary>
    /// <param name="className">The CSS class name to add.</param>
    /// <returns>A new class builder instance.</returns>
    public static IClassBuilder With(string? className) => new ClassBuilderInstance().With(className);

    /// <summary>
    /// Creates a new class builder instance with a conditional CSS class name.
    /// </summary>
    /// <param name="predicate">The condition that determines if the class should be applied.</param>
    /// <param name="className">The CSS class name to add when the predicate is true.</param>
    /// <returns>A new class builder instance.</returns>
    public static IClassBuilder With(Func<bool> predicate, string? className) =>
        new ClassBuilderInstance().With(predicate, className);

    /// <summary>
    /// Creates a new class builder instance with a dynamic CSS class name fetcher.
    /// </summary>
    /// <param name="fetch">The function that returns the CSS class name.</param>
    /// <returns>A new class builder instance.</returns>
    public static IClassBuilder With(Func<string?> fetch) => new ClassBuilderInstance().With(fetch);

    /// <summary>
    /// Creates a new class builder instance with a conditional dynamic CSS class name fetcher.
    /// </summary>
    /// <param name="predicate">The condition that determines if the class should be fetched.</param>
    /// <param name="fetch">The function that returns the CSS class name when the predicate is true.</param>
    /// <returns>A new class builder instance.</returns>
    public static IClassBuilder With(Func<bool> predicate, Func<string?> fetch) =>
        new ClassBuilderInstance().With(predicate, fetch);

    /// <summary>
    /// Creates a new class builder instance with dictionary-based CSS class lookup.
    /// </summary>
    /// <typeparam name="TK">The type of the dictionary key.</typeparam>
    /// <param name="key">The key to look up in the dictionary.</param>
    /// <param name="dictionary">The dictionary mapping keys to CSS class names.</param>
    /// <returns>A new class builder instance.</returns>
    public static IClassBuilder With<TK>(TK key, IDictionary<TK, string?> dictionary) =>
        new ClassBuilderInstance().With(key, dictionary);
}
