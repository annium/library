using System.IO;
using System.Linq;
using Annium.Core.Primitives;
using XRest.Clients.TypeScript.Views;
using XRest.Core.Components;
using XRest.Core.Models;

namespace XRest.Clients.TypeScript.Components.Implementations
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

            if (api.SharedExports.Count > 0)
                Write(output, "shared.ts", "Templates.SharedExports", new { Exports = api.SharedExports });

            foreach (var controllerView in api.Controllers.Where(x => x.Actions.Count > 0))
            {
                var directory = Namespace.New(controllerView.Namespace).ToPath(output);
                Directory.CreateDirectory(directory);

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