using System;
using System.Collections.Generic;

namespace xrest.Tools
{
    public class ApiData
    {
        public IReadOnlyCollection<Type> SharedExports { get; set; } = Array.Empty<Type>();
        public IReadOnlyCollection<ControllerData> Services { get; set; } = Array.Empty<ControllerData>();
    }
}