using System.Collections.Generic;
using XRest.TypeScript.Views.Types;

namespace XRest.TypeScript.Views
{
    internal class ControllerView
    {
        public string Name { get; }
        public IReadOnlyCollection<TypeView> Imports { get; }
        public IReadOnlyCollection<ActionView> Actions { get; }
        public IReadOnlyCollection<TypeView> Exports { get; }

        public ControllerView(
            string name,
            IReadOnlyCollection<TypeView> imports,
            IReadOnlyCollection<ActionView> actions,
            IReadOnlyCollection<TypeView> exports
        )
        {
            Name = name;
            Imports = imports;
            Actions = actions;
            Exports = exports;
        }
    }
}