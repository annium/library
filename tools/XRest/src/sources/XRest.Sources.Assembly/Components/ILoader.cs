using System;
using System.Collections.Generic;
using XRest.Core.Models;

namespace XRest.Sources.Assembly.Components
{
    public interface ILoader
    {
        ApiModel Load(string assemblyPath);
    }
}