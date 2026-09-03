using System.Threading;
using System.Threading.Tasks;
using Annium.XRest.Core.Models;

namespace Annium.XRest.Clients.Shared.Components;

public interface IApiModelLoader
{
    Task<ApiModel> LoadAsync(ISourceLoaderConfiguration cfg, CancellationToken ct);
}
