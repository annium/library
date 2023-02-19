using System.Linq;
using System.Reflection;
using System.Text;
using Annium;
using Annium.Core.Runtime.Resources;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Xmg.Core.Helpers;

namespace Xmg.Core.Tools.Implementations;

internal class TemplateWriter : ITemplateWriter
{
    private readonly IResourceLoader _resourceLoader;

    public TemplateWriter(
        IResourceLoader resourceLoader
    )
    {
        _resourceLoader = resourceLoader;
    }

    public string Write<T>(string template, T data)
        where T : class
    {
        var scriptObject = new ScriptObject();
        scriptObject.Import(data);
        scriptObject.Import(typeof(StringExtensions));

        var ctx = new TemplateContext();
        ctx.PushGlobal(scriptObject);

        var templateAssembly = Assembly.GetCallingAssembly();
        ctx.TemplateLoader = new TemplateLoader(_resourceLoader, templateAssembly);

        var resource = _resourceLoader.Load(template, templateAssembly).Single();
        var content = Encoding.UTF8.GetString(resource.Content.Span);
        var tpl = Template.Parse(content, lexerOptions: new LexerOptions());

        return tpl.Render(ctx);
    }
}