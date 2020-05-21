using System.Collections.Generic;

namespace XRest.Core.Models
{
    public class ControllerModel
    {
        public string? Area { get; }
        public string Name { get; }
        public IReadOnlyCollection<ActionModel> Actions { get; }

        public ControllerModel(
            string? area,
            string name,
            IReadOnlyCollection<ActionModel> actions
        )
        {
            Area = area;
            Name = name;
            Actions = actions;
        }
    }
}