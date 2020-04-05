using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Annium.Extensions.Primitives;
using XRest.Core.Models;
using XRest.TypeScript.Views.Types;

namespace XRest.TypeScript.Views
{
    internal class ActionView
    {
        public string Name { get; }
        public string Method { get; }
        public string Path { get; }
        public IReadOnlyCollection<ParameterView> Parameters { get; }
        public IReadOnlyCollection<ParameterView> RouteParameters => Parameters.Where(x => x.Location == ParameterLocationEnum.Path).ToArray();
        public IReadOnlyCollection<ParameterView> QueryParameters => Parameters.Where(x => x.Location == ParameterLocationEnum.Query).ToArray();
        public bool HasBody => !(Body is null);
        public DefinedTypeView? Body { get; }
        public DefinedTypeView Response { get; }
        public AuthView Auth { get; }

        public ActionView(
            string name,
            HttpMethod method,
            string path,
            IReadOnlyCollection<ParameterView> parameters,
            DefinedTypeView? body,
            DefinedTypeView response,
            AuthView auth
        )
        {
            Name = name;
            Method = method.Method.CamelCase();
            Path = path;
            Parameters = parameters;
            Body = body;
            Response = response;
            Auth = auth;
        }

        public override string ToString() => $"{Method} {Path}";
    }
}