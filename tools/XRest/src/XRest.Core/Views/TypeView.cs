using System;

namespace XRest.Core.Views;

public class TypeView
{
    public string FullName { get; set; } = string.Empty;
    public TypeView[] GenericArguments { get; set; } = Array.Empty<TypeView>();
}