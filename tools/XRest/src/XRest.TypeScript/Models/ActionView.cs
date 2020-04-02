using System.Collections.Generic;
using System.Net.Http;

namespace XRest.TypeScript.Models
{
    internal class ActionView
    {
        public string Name { get; }
        public HttpMethod Method { get; }
        public string Path { get; }
        public IReadOnlyCollection<ParameterView> Parameters { get; }
        public TypeView? Body { get; }
        public TypeView? Response { get; }
        public AuthView Auth { get; }

        public ActionView(
            string name,
            HttpMethod method,
            string path,
            IReadOnlyCollection<ParameterView> parameters,
            TypeView? body,
            TypeView? response,
            AuthView auth
        )
        {
            Name = name;
            Method = method;
            Path = path;
            Parameters = parameters;
            Body = body;
            Response = response;
            Auth = auth;
        }

        public override string ToString() => $"{Method} {Path}";
    }
}