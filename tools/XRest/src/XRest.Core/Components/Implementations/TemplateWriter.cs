using System.IO;
using System.Linq;
using System.Reflection;
using Annium.Extensions.Primitives;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using XRest.Core.Helpers;

namespace XRest.Core.Components.Implementations
{
    internal class TemplateWriter : ITemplateWriter
    {
        public string Write<T>(string template, T data)
            where T : class
        {
            var scriptObject = new ScriptObject();
            scriptObject.Import(data);
            scriptObject.Import(typeof(StringExtensions));

            var ctx = new TemplateContext();
            ctx.PushGlobal(scriptObject);

            var templateAssembly = Assembly.GetCallingAssembly();
            ctx.TemplateLoader = new TemplateLoader(templateAssembly);

            var resource = ResourceLoader.Load(template, templateAssembly).Single();
            using var reader = new StreamReader(resource.Content);
            var tpl = Template.Parse(reader.ReadToEnd(),lexerOptions:new LexerOptions{});

            return tpl.Render(ctx);
        }
    }
}