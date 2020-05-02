using System.IO;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Resources;
using Annium.Extensions.Primitives;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Xmg.Helpers;

namespace Xmg.Tools.Implementations
{
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
            using var reader = new StreamReader(resource.Content);
            var tpl = Template.Parse(reader.ReadToEnd(), lexerOptions: new LexerOptions { });

            return tpl.Render(ctx);
        }
    }
}