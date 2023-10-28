namespace XRest.Clients.Csharp.Views.Models;

internal interface ITypeModelView : IModelView
{
    int ArgsCount { get; }
    string Args { get; }
    bool HasExtends { get; }
    string Extends { get; }
}
