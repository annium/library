using System;
using System.Collections.Generic;

namespace Annium.Blazor.Core.Tools;

/// <summary>
/// Interface for building CSS class strings with typed data support.
/// </summary>
/// <typeparam name="T">The type of data used for building CSS classes.</typeparam>
public interface IClassBuilder<T>
{
    /// <summary>
    /// Creates a clone of this class builder instance.
    /// </summary>
    /// <returns>A new class builder instance with the same rules.</returns>
    IClassBuilder<T> Clone();

    /// <summary>
    /// Builds the CSS class string using the provided data.
    /// </summary>
    /// <param name="data">The data to use for building the CSS class string.</param>
    /// <returns>The built CSS class string.</returns>
    string Build(T data);

    /// <summary>
    /// Adds a CSS class name to the builder.
    /// </summary>
    /// <param name="className">The CSS class name to add.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    IClassBuilder<T> With(string? className);

    /// <summary>
    /// Adds a conditional CSS class name to the builder.
    /// </summary>
    /// <param name="predicate">The condition that determines if the class should be applied.</param>
    /// <param name="className">The CSS class name to add when the predicate is true.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    IClassBuilder<T> With(Func<bool> predicate, string? className);

    /// <summary>
    /// Adds a data-dependent conditional CSS class name to the builder.
    /// </summary>
    /// <param name="predicate">The condition based on data that determines if the class should be applied.</param>
    /// <param name="className">The CSS class name to add when the predicate is true.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    IClassBuilder<T> With(Func<T, bool> predicate, string? className);

    /// <summary>
    /// Adds a dynamic CSS class name fetcher to the builder.
    /// </summary>
    /// <param name="fetch">The function that returns the CSS class name.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    IClassBuilder<T> With(Func<string?> fetch);

    /// <summary>
    /// Adds a data-dependent dynamic CSS class name fetcher to the builder.
    /// </summary>
    /// <param name="fetch">The function that takes data and returns the CSS class name.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    IClassBuilder<T> With(Func<T, string?> fetch);

    /// <summary>
    /// Adds a conditional dynamic CSS class name fetcher to the builder.
    /// </summary>
    /// <param name="predicate">The condition that determines if the class should be fetched.</param>
    /// <param name="fetch">The function that returns the CSS class name when the predicate is true.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    IClassBuilder<T> With(Func<bool> predicate, Func<string?> fetch);

    /// <summary>
    /// Adds a data-dependent conditional dynamic CSS class name fetcher to the builder.
    /// </summary>
    /// <param name="predicate">The condition based on data that determines if the class should be fetched.</param>
    /// <param name="fetch">The function that returns the CSS class name when the predicate is true.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    IClassBuilder<T> With(Func<T, bool> predicate, Func<string?> fetch);

    /// <summary>
    /// Adds a conditional data-dependent dynamic CSS class name fetcher to the builder.
    /// </summary>
    /// <param name="predicate">The condition that determines if the class should be fetched.</param>
    /// <param name="fetch">The function that takes data and returns the CSS class name when the predicate is true.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    IClassBuilder<T> With(Func<bool> predicate, Func<T, string?> fetch);

    /// <summary>
    /// Adds a data-dependent conditional and data-dependent dynamic CSS class name fetcher to the builder.
    /// </summary>
    /// <param name="predicate">The condition based on data that determines if the class should be fetched.</param>
    /// <param name="fetch">The function that takes data and returns the CSS class name when the predicate is true.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    IClassBuilder<T> With(Func<T, bool> predicate, Func<T, string?> fetch);

    /// <summary>
    /// Adds a dictionary-based CSS class lookup to the builder.
    /// </summary>
    /// <typeparam name="TK">The type of the dictionary key.</typeparam>
    /// <param name="getKey">The function that extracts the key from the data.</param>
    /// <param name="dictionary">The dictionary mapping keys to CSS class names.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    IClassBuilder<T> With<TK>(Func<T, TK> getKey, IDictionary<TK, string?> dictionary);
}

/// <summary>
/// Interface for building CSS class strings without typed data support.
/// </summary>
public interface IClassBuilder
{
    /// <summary>
    /// Creates a clone of this class builder instance.
    /// </summary>
    /// <returns>A new class builder instance with the same rules.</returns>
    IClassBuilder Clone();

    /// <summary>
    /// Builds the CSS class string.
    /// </summary>
    /// <returns>The built CSS class string.</returns>
    string Build();

    /// <summary>
    /// Adds a CSS class name to the builder.
    /// </summary>
    /// <param name="className">The CSS class name to add.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    IClassBuilder With(string? className);

    /// <summary>
    /// Adds a conditional CSS class name to the builder.
    /// </summary>
    /// <param name="predicate">The condition that determines if the class should be applied.</param>
    /// <param name="className">The CSS class name to add when the predicate is true.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    IClassBuilder With(Func<bool> predicate, string? className);

    /// <summary>
    /// Adds a dynamic CSS class name fetcher to the builder.
    /// </summary>
    /// <param name="fetch">The function that returns the CSS class name.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    IClassBuilder With(Func<string?> fetch);

    /// <summary>
    /// Adds a conditional dynamic CSS class name fetcher to the builder.
    /// </summary>
    /// <param name="predicate">The condition that determines if the class should be fetched.</param>
    /// <param name="fetch">The function that returns the CSS class name when the predicate is true.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    IClassBuilder With(Func<bool> predicate, Func<string?> fetch);

    /// <summary>
    /// Adds a dictionary-based CSS class lookup to the builder.
    /// </summary>
    /// <typeparam name="TK">The type of the dictionary key.</typeparam>
    /// <param name="key">The key to look up in the dictionary.</param>
    /// <param name="dictionary">The dictionary mapping keys to CSS class names.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    IClassBuilder With<TK>(TK key, IDictionary<TK, string?> dictionary);
}
