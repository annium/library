using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace XRest.Core.Helpers
{
    internal class TemplateLoader : ITemplateLoader
    {
        private readonly Assembly _templateAssembly;

        public TemplateLoader(Assembly templateAssembly)
        {
            _templateAssembly = templateAssembly;
        }

        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName) => templateName;

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            var resource = ResourceLoader.Load(templatePath, _templateAssembly).Single();

            using var reader = new StreamReader(resource.Content);

            return reader.ReadToEnd();
        }

        public async ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            var resource = ResourceLoader.Load(templatePath, _templateAssembly).Single();

            using var reader = new StreamReader(resource.Content);

            return await reader.ReadToEndAsync();
        }
    }
}