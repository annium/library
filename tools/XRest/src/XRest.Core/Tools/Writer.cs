using System.IO;
using System.Linq;
using Annium.Extensions.Primitives;
using Scriban;
using Scriban.Runtime;
using XRest.Core.Helpers;

namespace XRest.Core.Tools
{
    public class Writer
    {
        public string Write<T>(string template, T data)
            where T : class
        {
            var resource = ResourceLoader.Load(template).Single();
            var scriptObject = new ScriptObject();
            scriptObject.Import(data);
            scriptObject.Import(typeof(StringExtensions));
            var ctx = new TemplateContext();
            ctx.PushGlobal(scriptObject);

            using var reader = new StreamReader(resource.Content);
            var tpl = Template.Parse(reader.ReadToEnd());

            return tpl.Render(ctx);
        }
    }
}