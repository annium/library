using System.Collections.Generic;
using Annium.Net.Types.Models;

namespace Annium.XRest.Core.Models;

public sealed record ControllerModel(Namespace Namespace, string Name, IReadOnlyCollection<ActionModel> Actions)
{
    public override string ToString() => $"{Namespace} > {Name}";
}
