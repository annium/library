using System.Net.Http;
using Annium.Core.Mapper;

namespace XRest.Core.Views.Profiles
{
    public class HttpMethodProfile : Profile
    {
        public HttpMethodProfile()
        {
            Map<HttpMethod, string>(x => x.ToString());
            Map<string, HttpMethod>(x => new HttpMethod(x));
        }
    }
}