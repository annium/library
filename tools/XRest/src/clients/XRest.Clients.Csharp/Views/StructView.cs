using System.Collections.Generic;

namespace XRest.Clients.Csharp.Views;

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