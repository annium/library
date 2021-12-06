using System.Linq;
using System.Reflection;
using System.Text;
using Annium.Core.Runtime.Resources;
using Annium.Core.Primitives;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using XRest.Core.Helpers;

namespace XRest.Core.Components.Implementations;

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
        ctx.TemplateLoader = new TemplateLoader(templateAssembly, _resourceLoader);

        var resource = _resourceLoader.Load(template, templateAssembly).Single();
        var raw = Encoding.UTF8.GetString(resource.Content.Span);
        var tpl = Template.Parse(raw, lexerOptions: new LexerOptions());

        return tpl.Render(ctx);
    }
}