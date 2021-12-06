using XRest.Core.Models;

namespace XRest.Core.Views;

public class ParameterView
{
    public string Name { get; set; } = string.Empty;
    public ParameterLocationEnum Location { get; set; }
    public TypeView Type { get; set; } = default!;
}