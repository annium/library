using System.Collections.Generic;
using System.Linq;
using Annium.Core.Primitives.Linq;
using Annium.Net.Types.Extensions;
using XRest.Clients.TypeScript.Views.Types;

namespace XRest.Clients.TypeScript.Views;

internal class ControllerView
{
    public string ImportSource { get; }
    public string Namespace { get; }
    public string Name { get; }
    public IReadOnlyCollection<DefinedTypeView> Imports { get; }
    public IReadOnlyCollection<ActionView> Actions { get; }
    public IReadOnlyCollection<DefinedTypeView> Exports { get; }

    public ControllerView(
        string ns,
        string name,
        IReadOnlyCollection<DefinedTypeView> imports,
        IReadOnlyCollection<ActionView> actions,
        IReadOnlyCollection<DefinedTypeView> exports
    )
    {
        ImportSource = ns == string.Empty
            ? "./shared"
            : $"{ns.ToNamespace().Select(_ => "..").Join("/")}/shared";
        Namespace = ns;
        Name = name;
        Imports = imports;
        Actions = actions;
        Exports = exports;
    }

    public override string ToString() => Name;
}