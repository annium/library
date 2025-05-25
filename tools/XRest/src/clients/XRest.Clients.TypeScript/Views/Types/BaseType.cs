namespace XRest.Clients.TypeScript.Views.Types;

internal static class BaseType
{
    public static ClassView Boolean { get; } = new ClassView("boolean");
    public static ClassView String { get; } = new ClassView("string");
    public static ClassView Number { get; } = new ClassView("number");
    public static ClassView Date { get; } = new ClassView("Date");
    public static ClassView Object { get; } = new ClassView("Object");
    public static ClassView Void { get; } = new ClassView("void");
    public static ClassView Array { get; } = new ClassView("Array", [new GenericParameterView("T")]);
    public static ClassView Record { get; } =
        new ClassView("Record", [new GenericParameterView("TKey"), new GenericParameterView("TValue")]);
    public static ClassView Nullable { get; } = new ClassView("Nullable", [new GenericParameterView("T")]);
}
