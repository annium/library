using System.Collections.Generic;

namespace XRest.Core.Models
{
    public class ControllerModel
    {
        public string Name { get; }
        public IReadOnlyCollection<ActionModel> Actions { get; }

        public ControllerModel(
            string name,
            IReadOnlyCollection<ActionModel> actions
        )
        {
            Name = name;
            Actions = actions;
        }
    }
}