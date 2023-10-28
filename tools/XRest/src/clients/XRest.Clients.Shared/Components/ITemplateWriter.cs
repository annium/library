namespace XRest.Clients.Shared.Components;

public interface ITemplateWriter
{
    string Write<T>(string template, T data)
        where T : class;
}
