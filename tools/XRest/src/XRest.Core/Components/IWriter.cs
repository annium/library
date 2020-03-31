namespace XRest.Core.Components
{
    public interface IWriter
    {
        string Write<T>(string template, T data) where T : class;
    }
}