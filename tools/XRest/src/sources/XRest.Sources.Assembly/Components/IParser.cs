using System;
using System.Collections.Generic;
using XRest.Core.Models;

namespace XRest.Sources.Assembly.Components;

internal interface IParser
{
    ApiModel Parse(IReadOnlyCollection<Type> controllerTypes);
}