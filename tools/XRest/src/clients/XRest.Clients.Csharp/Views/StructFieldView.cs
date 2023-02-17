namespace XRest.Clients.Csharp.Views;

internal sealed record StructFieldView(
    string Type,
    string Name,
    bool HasDefault,
    string Default
);