using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Annium.Blazor.Core.Tools
{
    public class ClassBuilder<T>
    {
        private readonly List<Func<T, string>> _rules = new List<Func<T, string>>();

        public ClassBuilder()
        {
        }

        private ClassBuilder(IEnumerable<Func<T, string>> rules)
        {
            _rules = rules.ToList();
        }

        public ClassBuilder<T> Clone()
        {
            return new ClassBuilder<T>(_rules);
        }

        public string Build(T data)
        {
            return string.Join(" ", _rules.Select(x => x(data)).Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        public ClassBuilder<T> With(string className) =>
            Rule(_ => className);

        public ClassBuilder<T> With(Func<bool> predicate, string className) =>
            Rule(_ => predicate() ? className : string.Empty);

        public ClassBuilder<T> With(Func<T, bool> predicate, string className) =>
            Rule(x => predicate(x) ? className : string.Empty);

        public ClassBuilder<T> With(Func<string> fetch) =>
            Rule(_ => fetch());

        public ClassBuilder<T> With(Func<T, string> fetch) =>
            Rule(fetch);

        public ClassBuilder<T> With(Func<bool> predicate, Func<string> fetch) =>
            Rule(_ => predicate() ? fetch() : string.Empty);

        public ClassBuilder<T> With(Func<T, bool> predicate, Func<string> fetch) =>
            Rule(x => predicate(x) ? fetch() : string.Empty);

        public ClassBuilder<T> With(Func<bool> predicate, Func<T, string> fetch) =>
            Rule(x => predicate() ? fetch(x) : string.Empty);

        public ClassBuilder<T> With(Func<T, bool> predicate, Func<T, string> fetch) =>
            Rule(x => predicate(x) ? fetch(x) : string.Empty);

        public ClassBuilder<T> With<TK>(Func<T, TK> getKey, IDictionary<TK, string> dictionary) =>
            Rule(x => dictionary.TryGetValue(getKey(x), out var value) ? value : string.Empty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ClassBuilder<T> Rule(Func<T, string> process)
        {
            _rules.Add(process);

            return this;
        }
    }

    public class ClassBuilder
    {
        private readonly List<Func<string>> _rules = new List<Func<string>>();

        public ClassBuilder()
        {
        }

        private ClassBuilder(IEnumerable<Func<string>> rules)
        {
            _rules = rules.ToList();
        }

        public ClassBuilder Clone()
        {
            return new ClassBuilder(_rules);
        }

        public string Build()
        {
            return string.Join(" ", _rules.Select(x => x()).Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        public ClassBuilder With(string className) =>
            Rule(() => className);

        public ClassBuilder With(Func<bool> predicate, string className) =>
            Rule(() => predicate() ? className : string.Empty);

        public ClassBuilder With(Func<string> fetch) =>
            Rule(fetch);

        public ClassBuilder With(Func<bool> predicate, Func<string> fetch) =>
            Rule(() => predicate() ? fetch() : string.Empty);

        public ClassBuilder With<TK>(TK key, IDictionary<TK, string> dictionary) =>
            Rule(() => dictionary.TryGetValue(key, out var value) ? value : string.Empty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ClassBuilder Rule(Func<string> process)
        {
            _rules.Add(process);

            return this;
        }
    }
}