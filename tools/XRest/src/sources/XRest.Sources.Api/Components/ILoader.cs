using System;
using System.Threading.Tasks;
using XRest.Core.Models;

namespace XRest.Sources.Api.Components;

public interface ILoader
{
    Task<ApiModel> Load(Uri apiUri, string assemblyPath);
}