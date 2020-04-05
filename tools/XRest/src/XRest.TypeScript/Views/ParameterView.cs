using XRest.Core.Models;

namespace XRest.TypeScript.Views
{
    internal class ParameterView
    {
        public string Name { get; }
        public ParameterLocationEnum Location { get; }
        public TypeView Type { get; }

        public ParameterView(
            string name,
            ParameterLocationEnum location,
            TypeView type
        )
        {
            Name = name;
            Location = location;
            Type = type;
        }
    }
}