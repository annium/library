namespace XRest.TypeScript.Models
{
    internal class TypePropertyView
    {
        public string Name { get; }
        public TypeReferenceView Type { get; }
        public bool IsOptional { get; }

        public TypePropertyView(
            string name,
            TypeReferenceView type,
            bool isOptional
        )
        {
            Name = name;
            Type = type;
            IsOptional = isOptional;
        }
    }
}