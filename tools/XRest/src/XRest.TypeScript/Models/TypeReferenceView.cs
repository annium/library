namespace XRest.TypeScript.Models
{
    internal class TypeReferenceView
    {
        public TypeView Type { get; }
        public bool IsNullable { get; }

        public TypeReferenceView(
            TypeView type,
            bool isNullable
        )
        {
            Type = type;
            IsNullable = isNullable;
        }
    }
}