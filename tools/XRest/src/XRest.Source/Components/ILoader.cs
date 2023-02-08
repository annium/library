using System.Threading.Tasks;
using XRest.Core.Models;

namespace XRest.Source.Components;

public interface ILoader
{
    Task<ApiModel> Load(ISourceLoaderConfiguration cfg);
}