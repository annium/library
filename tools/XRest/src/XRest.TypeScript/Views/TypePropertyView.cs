namespace XRest.TypeScript.Views
{
    internal class TypePropertyView
    {
        public string Name { get; }
        public TypeView Type { get; }
        public bool IsOptional { get; }

        public TypePropertyView(
            string name,
            TypeView type,
            bool isOptional
        )
        {
            Name = name;
            Type = type;
            IsOptional = isOptional;
        }

        public override string ToString() => $"{Type} {Name}";
    }
}