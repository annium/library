using System.Collections.Generic;

namespace XRest.Clients.Csharp.Views;

internal interface ITypeModelView : IModelView
{
    int ArgsCount { get; }
    string Args { get; }
    bool HasExtends { get; }
    string Extends { get; }
    IReadOnlyList<FieldView> Fields { get; }
}