using XRest.Core.Models;
using XRest.TypeScript.Views.Types;

namespace XRest.TypeScript.Views
{
    internal class ParameterView
    {
        public string Name { get; }
        public ParameterLocationEnum Location { get; }
        public DefinedTypeView Type { get; }

        public ParameterView(
            string name,
            ParameterLocationEnum location,
            DefinedTypeView type
        )
        {
            Name = name;
            Location = location;
            Type = type;
        }
    }
}