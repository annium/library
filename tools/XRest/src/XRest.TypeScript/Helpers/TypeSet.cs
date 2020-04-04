using System;
using System.Collections.Generic;
using System.Linq;
using XRest.TypeScript.Models;

namespace XRest.TypeScript.Helpers
{
    internal class TypeSet
    {
        private readonly HashSet<Type> _set = new HashSet<Type>();

        public TypeSet Register(Type type)
        {
            _set.Add(type);

            return this;
        }

        public bool Contains(Type type) => _set.Any(x => x.FullName == type.FullName);
    }
}