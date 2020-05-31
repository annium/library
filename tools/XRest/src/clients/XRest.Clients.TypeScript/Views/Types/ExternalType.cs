namespace XRest.Clients.TypeScript.Views.Types
{
    internal static class ExternalType
    {
        public static ClassView HttpResponseVoid { get; } = new ClassView("HttpResponse");
        public static ClassView HttpResponse { get; } = new ClassView("HttpResponse", new[] { new GenericParameterView("T") });
    }
}