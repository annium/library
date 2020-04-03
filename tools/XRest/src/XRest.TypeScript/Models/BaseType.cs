namespace XRest.TypeScript.Models
{
    internal static class BaseType
    {
        public static TypeView Boolean { get; } = new TypeView("boolean");
        public static TypeView String { get; } = new TypeView("string");
        public static TypeView Number { get; } = new TypeView("number");
        public static TypeView Date { get; } = new TypeView("Date");
    }
}