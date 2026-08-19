using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Annium.Core.Runtime.Resources;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Annium.XRest.Clients.Shared.Internal.Components;

internal class TemplateLoader : ITemplateLoader
{
    private readonly Assembly _templateAssembly;
    private readonly IResourceLoader _resourceLoader;

    public TemplateLoader(Assembly templateAssembly, IResourceLoader resourceLoader)
    {
        _templateAssembly = templateAssembly;
        _resourceLoader = resourceLoader;
    }

    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName) => templateName;

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        // the same named failure Write already reports for the top-level template: `.Single()` on a
        // miss says only "Sequence contains no elements", which Scriban then wraps, leaving nothing
        // that names the include or the assembly it was looked for in
        // the trailing dot forces a name-segment boundary, as Write does: the loader matches by bare
        // prefix, so `Templates.Client` would otherwise also match `Templates.ClientContainer.hbs`
        var resources = _resourceLoader.Load($"{templatePath}.", _templateAssembly);
        if (resources.Count != 1)
            throw new InvalidOperationException(
                $"Expected exactly one embedded template matching '{templatePath}' in {_templateAssembly.GetName().Name}, found {resources.Count}"
            );

        return Encoding.UTF8.GetString(resources.Single().Content.Span);
    }

    public ValueTask<string?> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath) =>
        new(Load(context, callerSpan, templatePath));
}
