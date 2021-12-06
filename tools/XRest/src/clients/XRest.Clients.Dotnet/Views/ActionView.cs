using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Annium.Core.Primitives;

namespace XRest.Clients.Dotnet.Views;

internal class ActionView
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
    public string ResponseDefault { get; }

    public ActionView(
        string name,
        HttpMethod method,
        string path,
        IReadOnlyCollection<ParameterView> pathParameters,
        IReadOnlyCollection<ParameterView> queryParameters,
        string body,
        string response,
        string responseDefault
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
        ResponseDefault = responseDefault;
        if (HasBody)
            Parameters = pathParameters.Concat(queryParameters).Append(new ParameterView("body", Body!)).ToArray();
        else
            Parameters = pathParameters.Concat(queryParameters).ToArray();
    }

    public override string ToString() => $"{Method} {Path}";
}