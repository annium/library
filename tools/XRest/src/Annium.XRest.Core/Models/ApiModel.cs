using System.Collections.Generic;
using Annium.Net.Types.Models;

namespace Annium.XRest.Core.Models;

public sealed record ApiModel(IReadOnlyCollection<ControllerModel> Controllers, IReadOnlyCollection<IModel> Models);
