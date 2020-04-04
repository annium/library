namespace XRest.TypeScript.Models
{
    internal static class BaseType
    {
        public static TypeView Boolean { get; } = new TypeView("boolean", false);
        public static TypeView String { get; } = new TypeView("string", false);
        public static TypeView Number { get; } = new TypeView("number", false);
        public static TypeView Date { get; } = new TypeView("Date", false);
        public static TypeView Object { get; } = new TypeView("Object", false);
        public static TypeView Void { get; } = new TypeView("void", false);
        public static TypeView Array { get; } = new TypeView("Array", new[] { new TypeView("T", true) });
        public static TypeView Record { get; } = new TypeView("Record", new[] { new TypeView("TKey", true), new TypeView("TValue", true) });
    }
}