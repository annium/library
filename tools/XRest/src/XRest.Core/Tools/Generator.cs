using System.IO;
using Annium.Logging.Abstractions;

namespace xrest.Tools
{
    public class Generator
    {
        private readonly Writer writer;
        private readonly ILogger<Generator> logger;

        public Generator(
            Writer writer,
            ILogger<Generator> logger
        )
        {
            this.writer = writer;
            this.logger = logger;
        }

        public void Generate(ApiData data, string output)
        {
            if (Directory.Exists(output))
                Directory.Delete(output, true);
            Directory.CreateDirectory(output);

            Write(output, "models.ts", "Templates.SharedExports", new { Exports = data.SharedExports });
            // Console.WriteLine("Shared exports");
            // foreach (var method in data.SharedExports)
            //     Console.WriteLine($"- share {method.Name}");
            // Console.WriteLine("Services");
            // foreach (var service in data.Services)
            // {
            //     Console.WriteLine($"Service '{service.Name}'");
            //     foreach (var import in service.Imports)
            //         Console.WriteLine($"- import {import.Name}");
            //     foreach (var method in service.Methods)
            //         Console.WriteLine($"- method {method.Name}");
            //     foreach (var export in service.Exports)
            //         Console.WriteLine($"- export {export.Name}");
            // }
        }

        private void Write<T>(string output, string file, string template, T data)
            where T : class
        {
            File.WriteAllText(Path.Combine(output, file), writer.Write(template, data));
        }
    }
}