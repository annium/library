using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Annium.Blazor.Core.Tools
{
    public class ClassBuilder<T>
    {
        private readonly List<Func<T, string>> _rules = new List<Func<T, string>>();

        public ClassBuilder<T> With(string className) => Rule(_ => className);

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

        public string Build(T data)
        {
            return string.Join(" ", _rules.Select(x => x(data)).Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ClassBuilder<T> Rule(Func<T, string> process)
        {
            _rules.Add(process);

            return this;
        }
    }
}