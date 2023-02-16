using System.Collections.Generic;

namespace XRest.Clients.Csharp.Views;

internal interface ITypeModelView : IModelView
{
    IReadOnlyList<string> Usages { get; }
    int ArgsCount { get; }
    string Args { get; }
    bool HasExtends { get; }
    string Extends { get; }
    IReadOnlyList<FieldView> Fields { get; }
}