using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Annium.Linq;
using Annium.Net.Types.Refs;

namespace XRest.Core.Models;

public sealed record ActionModel(
    HttpMethod Method,
    string Path,
    string Name,
    IReadOnlyCollection<ParameterModel> Parameters,
    IRef? Body,
    IRef Response
)
{
    public override string ToString()
    {
        var parameters = Parameters.Select(x => x.ToString()).ToList();
        if (Body is not null)
            parameters.Add($"body: {Body}");

        return $"{Method} {Path} {Name}({parameters.Join(", ")}): {Response}";
    }
}