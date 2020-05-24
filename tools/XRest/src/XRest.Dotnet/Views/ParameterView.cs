using XRest.Core.Models;

namespace XRest.Dotnet.Views
{
    internal class ParameterView
    {
        public string Name { get; }
        public string Type { get; }

        public ParameterView(
            string name,
            string type
        )
        {
            Name = name;
            Type = type;
        }

        public override string ToString() => $"{Type} {Name}";
    }
}