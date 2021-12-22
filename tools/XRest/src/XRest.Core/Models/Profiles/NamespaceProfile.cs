using Annium.Core.Mapper;

namespace XRest.Core.Models.Profiles;

public class NamespaceProfile : Profile
{
    public NamespaceProfile()
    {
        Map<Namespace, string>(x => x.ToString());
        Map<string, Namespace>(x => Namespace.New(x));
    }
}