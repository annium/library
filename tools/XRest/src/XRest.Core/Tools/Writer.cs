using System.IO;
using System.Linq;
using Annium.Extensions.Primitives;
using Annium.Logging.Abstractions;
using Scriban;
using Scriban.Runtime;

namespace xrest.Tools
{
    public class Writer
    {
        private readonly ILogger<Generator> logger;

        public Writer(ILogger<Generator> logger)
        {
            this.logger = logger;
        }

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