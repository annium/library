using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Annium.XRest.Clients.TypeScript.Views.Types;
using Annium.XRest.Core.Models;

namespace Annium.XRest.Clients.TypeScript.Views;

internal class ActionView
{
    public string Name { get; }
    public string Method { get; }
    public string Path { get; }
    public IReadOnlyCollection<ParameterView> Parameters { get; }
    public IReadOnlyCollection<ParameterView> RouteParameters =>
        Parameters.Where(x => x.Location == ParameterLocationEnum.Path).ToArray();
    public IReadOnlyCollection<ParameterView> QueryParameters =>
        Parameters.Where(x => x.Location == ParameterLocationEnum.Query).ToArray();
    public bool HasBody => !(Body is null);
    public DefinedTypeView? Body { get; }
    public DefinedTypeView Response { get; }

    public ActionView(
        string name,
        HttpMethod method,
        string path,
        IReadOnlyCollection<ParameterView> parameters,
        DefinedTypeView? body,
        DefinedTypeView response
    )
    {
        Name = name;
        Method = method.Method.CamelCase();
        Path = path;
        Parameters = parameters;
        Body = body;
        Response = response;
    }

    public override string ToString() => $"{Method} {Path}";
}
