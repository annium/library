using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Annium.XRest.Clients.Csharp.Views.Api;

internal sealed record ActionView
{
    public string Name { get; }
    public string Method { get; }
    public string Path { get; }
    public IReadOnlyCollection<ParameterView> Parameters { get; }
    public IReadOnlyCollection<ParameterView> PathParameters { get; }
    public IReadOnlyCollection<ParameterView> QueryParameters { get; }
    public bool HasBody { get; }
    public string Body { get; }
    public bool HasResponse { get; }
    public string Response { get; }

    public ActionView(
        string name,
        HttpMethod method,
        string path,
        IReadOnlyCollection<ParameterView> pathParameters,
        IReadOnlyCollection<ParameterView> queryParameters,
        string body,
        string response
    )
    {
        Name = name;
        Method = method.Method.PascalCase();
        Path = path;
        PathParameters = pathParameters;
        QueryParameters = queryParameters;
        HasBody = !string.IsNullOrWhiteSpace(body);
        Body = body;
        HasResponse = !string.IsNullOrWhiteSpace(response);
        Response = response;
        var parameters = pathParameters.Concat(queryParameters).ToList();
        if (HasBody)
            parameters.Add(new ParameterView(Body, "body"));
        Parameters = parameters;
    }

    public override string ToString() => $"{Method} {Path}";
}
