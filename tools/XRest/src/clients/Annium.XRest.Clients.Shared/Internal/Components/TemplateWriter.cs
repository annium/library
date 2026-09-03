using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Annium.Core.Runtime.Resources;
using Annium.XRest.Clients.Shared.Components;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Annium.XRest.Clients.Shared.Internal.Components;

internal class TemplateWriter : ITemplateWriter
{
    private readonly IResourceLoader _resourceLoader;

    public TemplateWriter(IResourceLoader resourceLoader)
    {
        _resourceLoader = resourceLoader;
    }

    // GetCallingAssembly resolves the templates against the caller's assembly, and inlining this
    // frame would resolve them against the wrong one
    [MethodImpl(MethodImplOptions.NoInlining)]
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

        var resources = _resourceLoader.Load($"{template}.", templateAssembly);
        if (resources.Count != 1)
            throw new InvalidOperationException(
                $"Expected exactly one embedded template matching '{template}.' in {templateAssembly.GetName().Name}, found {resources.Count}"
            );

        var raw = Encoding.UTF8.GetString(resources.Single().Content.Span);
        var tpl = Template.Parse(raw, lexerOptions: new LexerOptions());
        if (tpl.HasErrors)
            throw new InvalidOperationException($"Template '{template}' failed to parse: {tpl.Messages}");

        return tpl.Render(ctx);
    }
}
