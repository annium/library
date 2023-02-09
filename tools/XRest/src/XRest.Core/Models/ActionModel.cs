using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Annium.Core.Primitives.Collections.Generic;
using XRest.Core.Types.Models;

namespace XRest.Core.Models;

public sealed record ActionModel(
    HttpMethod Method,
    string Path,
    string Name,
    IReadOnlyCollection<ParameterModel> Parameters,
    ITypeModel? Body,
    ITypeModel Response
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