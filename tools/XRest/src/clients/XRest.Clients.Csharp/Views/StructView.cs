using System.Collections.Generic;

namespace XRest.Clients.Csharp.Views;

internal sealed record StructView(
    IReadOnlyList<string> Usages,
    string Namespace,
    string Name,
    int ArgsCount,
    string Args,
    bool HasExtends,
    string Extends,
    IReadOnlyList<FieldView> Fields
) : ITypeModelView;