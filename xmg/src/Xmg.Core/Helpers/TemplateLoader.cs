using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Annium.Core.Runtime.Resources;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Xmg.Core.Helpers;

internal class TemplateLoader : ITemplateLoader
{
    private readonly IResourceLoader _resourceLoader;
    private readonly Assembly _templateAssembly;

    public TemplateLoader(
        IResourceLoader resourceLoader,
        Assembly templateAssembly
    )
    {
        _resourceLoader = resourceLoader;
        _templateAssembly = templateAssembly;
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