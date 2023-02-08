using System.Collections.Generic;

namespace XRest.Core.Models;

public sealed record ControllerModel(
    Namespace Namespace,
    string Name,
    IReadOnlyCollection<ActionModel> Actions
)
{
    public override string ToString() => $"{Namespace} > {Name}";
}