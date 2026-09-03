using System.Collections.Generic;
using Annium.XRest.Clients.Csharp.Views.Models.Fields;

namespace Annium.XRest.Clients.Csharp.Views.Models;

internal sealed record StructView(
    IReadOnlyList<string> Usages,
    string Namespace,
    bool IsAbstract,
    string Name,
    int ArgsCount,
    string Args,
    bool HasExtends,
    string Extends,
    IReadOnlyList<StructFieldView> Fields
) : ITypeModelView;
