using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Core.Runtime.Resources;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace XRest.Core.Helpers
{
    internal class TemplateLoader : ITemplateLoader
    {
        private readonly Assembly _templateAssembly;
        private readonly IResourceLoader _resourceLoader;

        public TemplateLoader(
            Assembly templateAssembly,
            IResourceLoader resourceLoader
        )
        {
            _templateAssembly = templateAssembly;
            _resourceLoader = resourceLoader;
        }

        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName) => templateName;

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            var resource = _resourceLoader.Load(templatePath, _templateAssembly).Single();

            using var reader = new StreamReader(resource.Content);

            return reader.ReadToEnd();
        }

        public async ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            var resource = _resourceLoader.Load(templatePath, _templateAssembly).Single();

            using var reader = new StreamReader(resource.Content);

            return await reader.ReadToEndAsync();
        }
    }
}