using System;
using System.Collections.Generic;

namespace Annium.XRest.Clients.TypeScript.Views.Types;

internal record EnumView : DefinedTypeView
{
    public override TypeViewEnum Type => TypeViewEnum.Enum;

    public IReadOnlyDictionary<string, object> Values { get; }

    public EnumView(string name, IReadOnlyDictionary<string, object> values)
        : base(name)
    {
        if (values.Count == 0)
            throw new ArgumentException($"Enum '{this}' values count must be greater than 0");

        Values = values;
    }

    public override string ToString() => Name;
}
