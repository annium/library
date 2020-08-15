using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Annium.Blazor.Core.Tools;

namespace Annium.Blazor.Core.Internal.Tools
{
    internal class ClassBuilderInstance<T> : IClassBuilder<T>
    {
        private readonly List<Func<T, string?>> _rules = new List<Func<T, string?>>();

        internal ClassBuilderInstance()
        {
        }

        private ClassBuilderInstance(IEnumerable<Func<T, string?>> rules)
        {
            _rules = rules.ToList();
        }

        public IClassBuilder<T> Clone()
        {
            return new ClassBuilderInstance<T>(_rules);
        }

        public string Build(T data)
        {
            return string.Join(" ", _rules.Select(x => x(data)).Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        public IClassBuilder<T> With(string? className) =>
            Rule(_ => className);

        public IClassBuilder<T> With(Func<bool> predicate, string? className) =>
            Rule(_ => predicate() ? className : string.Empty);

        public IClassBuilder<T> With(Func<T, bool> predicate, string? className) =>
            Rule(x => predicate(x) ? className : string.Empty);

        public IClassBuilder<T> With(Func<string?> fetch) =>
            Rule(_ => fetch());

        public IClassBuilder<T> With(Func<T, string?> fetch) =>
            Rule(fetch);

        public IClassBuilder<T> With(Func<bool> predicate, Func<string?> fetch) =>
            Rule(_ => predicate() ? fetch() : string.Empty);

        public IClassBuilder<T> With(Func<T, bool> predicate, Func<string?> fetch) =>
            Rule(x => predicate(x) ? fetch() : string.Empty);

        public IClassBuilder<T> With(Func<bool> predicate, Func<T, string?> fetch) =>
            Rule(x => predicate() ? fetch(x) : string.Empty);

        public IClassBuilder<T> With(Func<T, bool> predicate, Func<T, string?> fetch) =>
            Rule(x => predicate(x) ? fetch(x) : string.Empty);

        public IClassBuilder<T> With<TK>(Func<T, TK> getKey, IDictionary<TK, string?> dictionary) =>
            Rule(x => dictionary.TryGetValue(getKey(x), out var value) ? value : string.Empty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IClassBuilder<T> Rule(Func<T, string?> process)
        {
            _rules.Add(process);

            return this;
        }
    }

    internal class ClassBuilderInstance : IClassBuilder
    {
        private readonly List<Func<string?>> _rules = new List<Func<string?>>();

        internal ClassBuilderInstance()
        {
        }

        private ClassBuilderInstance(IEnumerable<Func<string?>> rules)
        {
            _rules = rules.ToList();
        }

        public IClassBuilder Clone()
        {
            return new ClassBuilderInstance(_rules);
        }

        public string Build()
        {
            return string.Join(" ", _rules.Select(x => x()).Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        public IClassBuilder With(string? className) =>
            Rule(() => className);

        public IClassBuilder With(Func<bool> predicate, string? className) =>
            Rule(() => predicate() ? className : string.Empty);

        public IClassBuilder With(Func<string?> fetch) =>
            Rule(fetch);

        public IClassBuilder With(Func<bool> predicate, Func<string?> fetch) =>
            Rule(() => predicate() ? fetch() : string.Empty);

        public IClassBuilder With<TK>(TK key, IDictionary<TK, string?> dictionary) =>
            Rule(() => dictionary.TryGetValue(key, out var value) ? value : string.Empty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IClassBuilder Rule(Func<string?> process)
        {
            _rules.Add(process);

            return this;
        }
    }
}