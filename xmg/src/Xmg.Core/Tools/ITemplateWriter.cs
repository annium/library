namespace Xmg.Core.Tools;

public interface ITemplateWriter
{
    string Write<T>(string template, T data)
        where T : class;
}
