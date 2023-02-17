namespace XRest.Clients.Csharp.Views.Models.Fields;

internal sealed record StructFieldView(
    string Type,
    string Name,
    bool HasDefault,
    string Default
) : IFieldView;