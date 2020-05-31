using System.Collections.Generic;
using XRest.Clients.TypeScript.Views.Types;

namespace XRest.Clients.TypeScript.Views
{
    internal class ControllerView
    {
        public string? Area { get; }
        public string Name { get; }
        public IReadOnlyCollection<DefinedTypeView> Imports { get; }
        public IReadOnlyCollection<ActionView> Actions { get; }
        public IReadOnlyCollection<DefinedTypeView> Exports { get; }

        public ControllerView(
            string? area,
            string name,
            IReadOnlyCollection<DefinedTypeView> imports,
            IReadOnlyCollection<ActionView> actions,
            IReadOnlyCollection<DefinedTypeView> exports
        )
        {
            Area = area;
            Name = name;
            Imports = imports;
            Actions = actions;
            Exports = exports;
        }

        public override string ToString() => Name;
    }
}