using System;
using System.Collections.Generic;
using System.Reflection;

namespace xrest.Tools
{
    public class ControllerData
    {
        public string Name { get; set; } = string.Empty;
        public IReadOnlyCollection<Type> Imports { get; set; } = Array.Empty<Type>();
        public IReadOnlyCollection<MethodInfo> Methods { get; set; } = Array.Empty<MethodInfo>();
        public IReadOnlyCollection<Type> Exports { get; set; } = Array.Empty<Type>();
    }
}