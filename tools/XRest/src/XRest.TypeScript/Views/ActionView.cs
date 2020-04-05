using System.Collections.Generic;
using System.Net.Http;
using XRest.TypeScript.Views.Types;

namespace XRest.TypeScript.Views
{
    internal class ActionView
    {
        public string Name { get; }
        public HttpMethod Method { get; }
        public string Path { get; }
        public IReadOnlyCollection<ParameterView> Parameters { get; }
        public DefinedTypeView? Body { get; }
        public DefinedTypeView? Response { get; }
        public AuthView Auth { get; }

        public ActionView(
            string name,
            HttpMethod method,
            string path,
            IReadOnlyCollection<ParameterView> parameters,
            DefinedTypeView? body,
            DefinedTypeView? response,
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