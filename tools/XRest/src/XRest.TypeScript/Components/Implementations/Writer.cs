using System.IO;
using System.Linq;
using Annium.Extensions.Primitives;
using XRest.Core.Components;
using XRest.TypeScript.Views;

namespace XRest.TypeScript.Components.Implementations
{
    internal class Writer : IWriter
    {
        private readonly ITemplateWriter _templateWriter;

        public Writer(
            ITemplateWriter templateWriter
        )
        {
            _templateWriter = templateWriter;
        }

        public void Write(string output, ApiView api)
        {
            if (Directory.Exists(output))
                Directory.Delete(output, true);
            Directory.CreateDirectory(output);

            Write(output, "shared.ts", "Templates.SharedExports", new { Exports = api.SharedExports });

            foreach (var group in api.Controllers.Where(x => x.Actions.Count > 0).GroupBy(x => x.Area))
            foreach (var controllerView in group)
            {
                var directory = output;
                if (!string.IsNullOrWhiteSpace(group.Key))
                {
                    directory = Path.Combine(output, group.Key);
                    Directory.CreateDirectory(directory);
                }

                Write(directory, $"{controllerView.Name.CamelCase()}Api.ts", "Templates.Api", controllerView);
            }
        }

        private void Write<T>(string output, string file, string template, T data)
            where T : class
        {
            File.WriteAllText(Path.Combine(output, file), _templateWriter.Write(template, data));
        }
    }
}