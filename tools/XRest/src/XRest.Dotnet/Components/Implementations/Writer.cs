using System.IO;
using XRest.Core.Components;
using XRest.Dotnet.Views;
using static XRest.Dotnet.Helpers.WriterHelper;

namespace XRest.Dotnet.Components.Implementations
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

        public void Write(string output, ClientContainerView client)
        {
            Write(output, "RequestFactory", "Templates.RequestFactory", new
            {
                Usages = new[] { "Annium.Net.Http" },
                Namespace = client.Namespace,
            });
            Write(output, "ClientBase", "Templates.ClientBase", new
            {
                Usages = new[] { "Annium.Net.Http" },
                Namespace = client.Namespace,
            });
            WriteClientContainer(output, client.Namespace, client);
        }

        private void WriteAbstractClient(string rootDir, string rootNs, IClientView abstraction)
        {
            switch (abstraction)
            {
                case ClientContainerView container:
                    WriteClientContainer(rootDir, rootNs, container);
                    break;
                case ClientView client:
                    WriteClient(rootDir, rootNs, client);
                    break;
            }
        }

        private void WriteClientContainer(string rootDir, string rootNs, ClientContainerView container)
        {
            var output = GetOutputPath(rootDir, rootNs, container.Namespace);
            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);

            Write(output, container.Type, "Templates.ClientContainer", container);

            foreach (var client in container.Clients)
                WriteAbstractClient(rootDir, rootNs, client);
        }

        private void WriteClient(string rootDir, string rootNs, ClientView client)
        {
            var output = GetOutputPath(rootDir, rootNs, client.Namespace);
            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);

            Write(output, client.Type, "Templates.Client", client);
        }

        private void Write<T>(string output, string fileName, string template, T data)
            where T : class
        {
            File.WriteAllText(Path.Combine(output, $"{fileName}.cs"), _templateWriter.Write(template, data));
        }
    }
}