using System;
using System.Collections.Generic;
using XRest.Core.Models;

namespace XRest.TypeScript.Models
{
    public class ControllerView
    {
        public string Name { get; }
        public IReadOnlyCollection<Type> Imports { get; }
        public IReadOnlyCollection<ActionModel> Actions { get; }
        public IReadOnlyCollection<Type> Exports { get; }

        public ControllerView(
            string name,
            IReadOnlyCollection<Type> imports,
            IReadOnlyCollection<ActionModel> actions,
            IReadOnlyCollection<Type> exports
        )
        {
            Name = name;
            Imports = imports;
            Actions = actions;
            Exports = exports;
        }
    }
}