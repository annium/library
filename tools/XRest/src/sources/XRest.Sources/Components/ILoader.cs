using System.Threading.Tasks;
using XRest.Core.Models;

namespace XRest.Sources.Components
{
    public interface ILoader
    {
        Task<ApiModel> Load(ISourceLoaderConfiguration cfg);
    }
}