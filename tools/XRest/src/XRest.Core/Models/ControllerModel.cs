using System.Collections.Generic;

namespace XRest.Core.Models
{
    public class ControllerModel
    {
        public Namespace Namespace { get; }
        public string Name { get; }
        public IReadOnlyCollection<ActionModel> Actions { get; }

        public ControllerModel(
            Namespace @namespace,
            string name,
            IReadOnlyCollection<ActionModel> actions
        )
        {
            Namespace = @namespace;
            Name = name;
            Actions = actions;
        }
    }
}