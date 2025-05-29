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
        var resource = _resourceLoader.Load(templatePath, _templateAssembly).Single();

        return Encoding.UTF8.GetString(resource.Content.Span);
    }

    public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        var resource = _resourceLoader.Load(templatePath, _templateAssembly).Single();

        return new ValueTask<string>(Encoding.UTF8.GetString(resource.Content.Span));
    }
}
