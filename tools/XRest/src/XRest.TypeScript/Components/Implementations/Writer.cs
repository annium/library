using System.IO;
using System.Linq;
using Annium.Logging.Abstractions;
using XRest.Core.Components;
using XRest.TypeScript.Views;
using XRest.TypeScript.Views.Types;

namespace XRest.TypeScript.Components.Implementations
{
    internal class Writer : IWriter
    {
        private readonly ILogger<Writer> _logger;
        private readonly ITemplateWriter _templateWriter;

        public Writer(
            ILogger<Writer> logger,
            ITemplateWriter templateWriter
        )
        {
            _logger = logger;
            _templateWriter = templateWriter;
        }

        public void Write(string output, ApiView api)
        {
            if (Directory.Exists(output))
                Directory.Delete(output, true);
            Directory.CreateDirectory(output);

            var data = new
            {
                Interfaces = api.SharedExports.OfType<ClassView>(),
                Enums = api.SharedExports.OfType<EnumView>(),
            };
            Write(output, "shared.ts", "Templates.SharedExports", data);
        }


        private void Write<T>(string output, string file, string template, T data)
            where T : class
        {
            File.WriteAllText(Path.Combine(output, file), _templateWriter.Write(template, data));
        }
    }
}