using Annium.Core.Mapper;
using XRest.Core.Extensions;

namespace XRest.Core.Models.Profiles;

public class NamespaceProfile : Profile
{
    public NamespaceProfile()
    {
        Map<Namespace, string>(x => x.ToString());
        Map<string, Namespace>(x => x.ToNamespace());
    }
}