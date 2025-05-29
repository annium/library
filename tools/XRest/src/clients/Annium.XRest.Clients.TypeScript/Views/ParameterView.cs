using Annium.XRest.Clients.TypeScript.Views.Types;
using Annium.XRest.Core.Models;

namespace Annium.XRest.Clients.TypeScript.Views;

internal class ParameterView
{
    public string Name { get; }
    public ParameterLocationEnum Location { get; }
    public DefinedTypeView Type { get; }

    public ParameterView(string name, ParameterLocationEnum location, DefinedTypeView type)
    {
        Name = name;
        Location = location;
        Type = type;
    }

    public override string ToString() => $"[{Location}] {Type} {Name}";
}
