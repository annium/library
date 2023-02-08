using System.Collections.Generic;

namespace XRest.Core.Models;

public sealed record ApiModel(IReadOnlyCollection<ControllerModel> Controllers);