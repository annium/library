using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Annium.Blazor.Core.Tools;

namespace Annium.Blazor.Core.Internal.Tools;

/// <summary>
/// Internal implementation of IClassBuilder with typed data support for building CSS class strings.
/// </summary>
/// <typeparam name="T">The type of data used for building CSS classes.</typeparam>
internal class ClassBuilderInstance<T> : IClassBuilder<T>
{
    /// <summary>
    /// List of rules that define how CSS classes are built based on the provided data.
    /// </summary>
    private readonly List<Func<T, string?>> _rules = new();

    /// <summary>
    /// Initializes a new instance of the ClassBuilderInstance class.
    /// </summary>
    internal ClassBuilderInstance() { }

    /// <summary>
    /// Initializes a new instance of the ClassBuilderInstance class with existing rules.
    /// </summary>
    /// <param name="rules">The existing rules to copy.</param>
    private ClassBuilderInstance(IEnumerable<Func<T, string?>> rules)
    {
        _rules = rules.ToList();
    }

    /// <summary>
    /// Creates a clone of this class builder instance.
    /// </summary>
    /// <returns>A new class builder instance with the same rules.</returns>
    public IClassBuilder<T> Clone()
    {
        return new ClassBuilderInstance<T>(_rules);
    }

    /// <summary>
    /// Builds the CSS class string using the provided data.
    /// </summary>
    /// <param name="data">The data to use for building the CSS class string.</param>
    /// <returns>The built CSS class string with space-separated class names.</returns>
    public string Build(T data)
    {
        return string.Join(" ", _rules.Select(x => x(data)).Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    /// <summary>
    /// Adds a CSS class name to the builder.
    /// </summary>
    /// <param name="className">The CSS class name to add.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    public IClassBuilder<T> With(string? className) => Rule(_ => className);

    /// <summary>
    /// Adds a conditional CSS class name to the builder.
    /// </summary>
    /// <param name="predicate">The condition that determines if the class should be applied.</param>
    /// <param name="className">The CSS class name to add when the predicate is true.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    public IClassBuilder<T> With(Func<bool> predicate, string? className) =>
        Rule(_ => predicate() ? className : string.Empty);

    /// <summary>
    /// Adds a data-dependent conditional CSS class name to the builder.
    /// </summary>
    /// <param name="predicate">The condition based on data that determines if the class should be applied.</param>
    /// <param name="className">The CSS class name to add when the predicate is true.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    public IClassBuilder<T> With(Func<T, bool> predicate, string? className) =>
        Rule(x => predicate(x) ? className : string.Empty);

    /// <summary>
    /// Adds a dynamic CSS class name fetcher to the builder.
    /// </summary>
    /// <param name="fetch">The function that returns the CSS class name.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    public IClassBuilder<T> With(Func<string?> fetch) => Rule(_ => fetch());

    /// <summary>
    /// Adds a data-dependent dynamic CSS class name fetcher to the builder.
    /// </summary>
    /// <param name="fetch">The function that takes data and returns the CSS class name.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    public IClassBuilder<T> With(Func<T, string?> fetch) => Rule(fetch);

    /// <summary>
    /// Adds a conditional dynamic CSS class name fetcher to the builder.
    /// </summary>
    /// <param name="predicate">The condition that determines if the class should be fetched.</param>
    /// <param name="fetch">The function that returns the CSS class name when the predicate is true.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    public IClassBuilder<T> With(Func<bool> predicate, Func<string?> fetch) =>
        Rule(_ => predicate() ? fetch() : string.Empty);

    /// <summary>
    /// Adds a data-dependent conditional dynamic CSS class name fetcher to the builder.
    /// </summary>
    /// <param name="predicate">The condition based on data that determines if the class should be fetched.</param>
    /// <param name="fetch">The function that returns the CSS class name when the predicate is true.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    public IClassBuilder<T> With(Func<T, bool> predicate, Func<string?> fetch) =>
        Rule(x => predicate(x) ? fetch() : string.Empty);

    /// <summary>
    /// Adds a conditional data-dependent dynamic CSS class name fetcher to the builder.
    /// </summary>
    /// <param name="predicate">The condition that determines if the class should be fetched.</param>
    /// <param name="fetch">The function that takes data and returns the CSS class name when the predicate is true.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    public IClassBuilder<T> With(Func<bool> predicate, Func<T, string?> fetch) =>
        Rule(x => predicate() ? fetch(x) : string.Empty);

    /// <summary>
    /// Adds a data-dependent conditional and data-dependent dynamic CSS class name fetcher to the builder.
    /// </summary>
    /// <param name="predicate">The condition based on data that determines if the class should be fetched.</param>
    /// <param name="fetch">The function that takes data and returns the CSS class name when the predicate is true.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    public IClassBuilder<T> With(Func<T, bool> predicate, Func<T, string?> fetch) =>
        Rule(x => predicate(x) ? fetch(x) : string.Empty);

    /// <summary>
    /// Adds a dictionary-based CSS class lookup to the builder.
    /// </summary>
    /// <typeparam name="TK">The type of the dictionary key.</typeparam>
    /// <param name="getKey">The function that extracts the key from the data.</param>
    /// <param name="dictionary">The dictionary mapping keys to CSS class names.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    public IClassBuilder<T> With<TK>(Func<T, TK> getKey, IDictionary<TK, string?> dictionary) =>
        Rule(x => dictionary.TryGetValue(getKey(x), out var value) ? value : string.Empty);

    /// <summary>
    /// Adds a rule to the builder for processing CSS class names.
    /// </summary>
    /// <param name="process">The function that processes data to return a CSS class name.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IClassBuilder<T> Rule(Func<T, string?> process)
    {
        _rules.Add(process);

        return this;
    }
}

/// <summary>
/// Internal implementation of IClassBuilder without typed data support for building CSS class strings.
/// </summary>
internal class ClassBuilderInstance : IClassBuilder
{
    /// <summary>
    /// List of rules that define how CSS classes are built.
    /// </summary>
    private readonly List<Func<string?>> _rules = new();

    /// <summary>
    /// Initializes a new instance of the ClassBuilderInstance class.
    /// </summary>
    internal ClassBuilderInstance() { }

    /// <summary>
    /// Initializes a new instance of the ClassBuilderInstance class with existing rules.
    /// </summary>
    /// <param name="rules">The existing rules to copy.</param>
    private ClassBuilderInstance(IEnumerable<Func<string?>> rules)
    {
        _rules = rules.ToList();
    }

    /// <summary>
    /// Creates a clone of this class builder instance.
    /// </summary>
    /// <returns>A new class builder instance with the same rules.</returns>
    public IClassBuilder Clone()
    {
        return new ClassBuilderInstance(_rules);
    }

    /// <summary>
    /// Builds the CSS class string.
    /// </summary>
    /// <returns>The built CSS class string with space-separated class names.</returns>
    public string Build()
    {
        return string.Join(" ", _rules.Select(x => x()).Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    /// <summary>
    /// Adds a CSS class name to the builder.
    /// </summary>
    /// <param name="className">The CSS class name to add.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    public IClassBuilder With(string? className) => Rule(() => className);

    /// <summary>
    /// Adds a conditional CSS class name to the builder.
    /// </summary>
    /// <param name="predicate">The condition that determines if the class should be applied.</param>
    /// <param name="className">The CSS class name to add when the predicate is true.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    public IClassBuilder With(Func<bool> predicate, string? className) =>
        Rule(() => predicate() ? className : string.Empty);

    /// <summary>
    /// Adds a dynamic CSS class name fetcher to the builder.
    /// </summary>
    /// <param name="fetch">The function that returns the CSS class name.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    public IClassBuilder With(Func<string?> fetch) => Rule(fetch);

    /// <summary>
    /// Adds a conditional dynamic CSS class name fetcher to the builder.
    /// </summary>
    /// <param name="predicate">The condition that determines if the class should be fetched.</param>
    /// <param name="fetch">The function that returns the CSS class name when the predicate is true.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    public IClassBuilder With(Func<bool> predicate, Func<string?> fetch) =>
        Rule(() => predicate() ? fetch() : string.Empty);

    /// <summary>
    /// Adds a dictionary-based CSS class lookup to the builder.
    /// </summary>
    /// <typeparam name="TK">The type of the dictionary key.</typeparam>
    /// <param name="key">The key to look up in the dictionary.</param>
    /// <param name="dictionary">The dictionary mapping keys to CSS class names.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    public IClassBuilder With<TK>(TK key, IDictionary<TK, string?> dictionary) =>
        Rule(() => dictionary.TryGetValue(key, out var value) ? value : string.Empty);

    /// <summary>
    /// Returns the built CSS class string.
    /// </summary>
    /// <returns>The built CSS class string.</returns>
    public override string ToString() => Build();

    /// <summary>
    /// Adds a rule to the builder for processing CSS class names.
    /// </summary>
    /// <param name="process">The function that returns a CSS class name.</param>
    /// <returns>The current class builder instance for method chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IClassBuilder Rule(Func<string?> process)
    {
        _rules.Add(process);

        return this;
    }
}
