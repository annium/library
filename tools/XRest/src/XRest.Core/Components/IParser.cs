using System;
using System.Collections.Generic;
using XRest.Core.Models;

namespace XRest.Core.Components
{
    public interface IParser
    {
        ApiModel Parse(IReadOnlyCollection<Type> controllerTypes);
    }
}