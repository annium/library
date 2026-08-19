using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Annium.XRest.Clients.Csharp.Components.Processors;

namespace Annium.XRest.Clients.Csharp.Views.Api;

internal sealed record ActionView
{
    public string Name { get; }
    public string Method { get; }
    public string Path { get; }
    public IReadOnlyCollection<ParameterView> Parameters { get; }
    public string BodyArgument { get; }
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
        HasBody = !string.IsNullOrWhiteSpace(body);
        Body = body;
        HasResponse = !string.IsNullOrWhiteSpace(response);
        Response = response;
        // the path parameters are interpolated into the route, so their identifiers are fixed; every
        // other one gives way when the name is already taken. Without this a route parameter named
        // `body`, or a flattened query object repeating a name, declared the same identifier twice
        // (CS0100)
        var taken = pathParameters.Select(x => x.Argument).ToHashSet();
        var parameters = pathParameters.ToList();
        parameters.AddRange(queryParameters.Select(x => Disambiguate(x, taken)));

        var bodyParameter = HasBody ? Disambiguate(new ParameterView(Body, "body"), taken) : null;
        BodyArgument = bodyParameter?.Argument ?? string.Empty;
        if (bodyParameter is not null)
            parameters.Add(bodyParameter);

        QueryParameters = parameters
            .Take(pathParameters.Count + queryParameters.Count)
            .Skip(pathParameters.Count)
            .ToArray();
        Parameters = parameters;
    }

    private static ParameterView Disambiguate(ParameterView parameter, HashSet<string> taken) =>
        parameter with
        {
            Argument = Naming.Take(parameter.Argument, taken),
        };

    public override string ToString() => $"{Method} {Path}";
}
