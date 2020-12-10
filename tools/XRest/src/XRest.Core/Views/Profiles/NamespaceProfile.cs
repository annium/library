using Annium.Core.Mapper;
using XRest.Core.Models;

namespace XRest.Core.Views.Profiles
{
    public class NamespaceProfile : Profile
    {
        public NamespaceProfile()
        {
            Map<Namespace, string>(x => x.ToString());
            Map<string, Namespace>(x => Namespace.New(x));
        }
    }
}