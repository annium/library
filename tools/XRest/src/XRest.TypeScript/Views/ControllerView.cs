using System.Collections.Generic;
using XRest.TypeScript.Views.Types;

namespace XRest.TypeScript.Views
{
    internal class ControllerView
    {
        public string Name { get; }
        public IReadOnlyCollection<DefinedTypeView> Imports { get; }
        public IReadOnlyCollection<ActionView> Actions { get; }
        public IReadOnlyCollection<DefinedTypeView> Exports { get; }

        public ControllerView(
            string name,
            IReadOnlyCollection<DefinedTypeView> imports,
            IReadOnlyCollection<ActionView> actions,
            IReadOnlyCollection<DefinedTypeView> exports
        )
        {
            Name = name;
            Imports = imports;
            Actions = actions;
            Exports = exports;
        }
    }
}