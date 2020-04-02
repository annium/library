using System.Collections.Generic;

namespace XRest.TypeScript.Models
{
    internal class ApiView
    {
        public IReadOnlyCollection<TypeView> SharedExports { get; }
        public IReadOnlyCollection<ControllerView> Controllers { get; }

        public ApiView(
            IReadOnlyCollection<TypeView> sharedExports,
            IReadOnlyCollection<ControllerView> controllers
        )
        {
            SharedExports = sharedExports;
            Controllers = controllers;
        }
    }
}