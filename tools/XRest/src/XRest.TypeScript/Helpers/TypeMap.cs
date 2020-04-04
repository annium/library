using System;
using System.Collections.Generic;
using System.Linq;
using XRest.TypeScript.Models;

namespace XRest.TypeScript.Helpers
{
    internal class TypeMap
    {
        private readonly IDictionary<Type, TypeView> _map = new Dictionary<Type, TypeView>();

        public TypeMap Register(Type type, TypeView view)
        {
            _map[type] = view;

            return this;
        }

        public bool Contains(Type type) => _map.Keys.Any(Matcher(type));

        public bool Contains(TypeView view) => _map.Values.Contains(view);

        public TypeView Get(Type type) => _map[_map.Keys.Single(Matcher(type))];

        public bool TryGet(Type type, out TypeView? view)
        {
            var key = _map.Keys.SingleOrDefault(Matcher(type));
            view = key is null ? null : _map[key];

            return key != null;
        }

        private Func<Type, bool> Matcher(Type type) => x => x.FullName == type.FullName;
    }
}