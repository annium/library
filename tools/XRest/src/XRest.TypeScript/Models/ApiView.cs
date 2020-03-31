using System;
using System.Collections.Generic;

namespace XRest.TypeScript.Models
{
    internal class ApiView
    {
        public IReadOnlyCollection<Type> SharedExports { get; }
        public IReadOnlyCollection<ControllerView> Controllers { get; }

        public ApiView(
            IReadOnlyCollection<Type> sharedExports,
            IReadOnlyCollection<ControllerView> controllers
        )
        {
            SharedExports = sharedExports;
            Controllers = controllers;
        }
    }
}