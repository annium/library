using System;
using System.Collections.Generic;
using System.Linq;
using XRest.Clients.TypeScript.Views.Types;

namespace XRest.Clients.TypeScript.Helpers;

internal class TypeMap
{
    private readonly IDictionary<Type, DefinedTypeView> _map = new Dictionary<Type, DefinedTypeView>();

    public TypeMap Register(Type type, DefinedTypeView view)
    {
        _map[type] = view;

        return this;
    }

    public bool Contains(Type type) => _map.Keys.Any(Matcher(type));

    public bool Contains(DefinedTypeView view) => _map.Values.Contains(view);

    public DefinedTypeView Get(Type type) => _map[_map.Keys.Single(Matcher(type))];

    public bool TryGet(Type type, out DefinedTypeView? view)
    {
        var key = _map.Keys.SingleOrDefault(Matcher(type));
        view = key is null ? null : _map[key];

        return key != null;
    }

    private Func<Type, bool> Matcher(Type type) => x => x.FullName == type.FullName;
}