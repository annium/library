using System.Collections.Generic;

namespace XRest.Core.Models
{
    public class ApiModel
    {
        public IReadOnlyCollection<ControllerModel> Controllers { get; }

        public ApiModel(
            IReadOnlyCollection<ControllerModel> controllers
        )
        {
            Controllers = controllers;
        }
    }
}