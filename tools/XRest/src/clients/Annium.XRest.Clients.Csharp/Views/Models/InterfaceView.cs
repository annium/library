using System.Collections.Generic;
using Annium.XRest.Clients.Csharp.Views.Models.Fields;

namespace Annium.XRest.Clients.Csharp.Views.Models;

internal sealed record InterfaceView(
    IReadOnlyList<string> Usages,
    string Namespace,
    string Name,
    int ArgsCount,
    string Args,
    bool HasExtends,
    string Extends,
    IReadOnlyList<InterfaceFieldView> Fields
) : ITypeModelView;
