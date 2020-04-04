using System.IO;
using Annium.Logging.Abstractions;
using XRest.Core.Components;
using XRest.TypeScript.Models;

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

            Write(output, "shared.ts", "Templates.SharedExports", new { Exports = api.SharedExports });
        }


        private void Write<T>(string output, string file, string template, T data)
            where T : class
        {
            File.WriteAllText(Path.Combine(output, file), _templateWriter.Write(template, data));
        }
    }
}