using System.Threading.Tasks;
using XRest.Core.Models;

namespace XRest.Clients.Shared.Components;

public interface IApiModelLoader
{
    Task<ApiModel> Load(ISourceLoaderConfiguration cfg);
}
