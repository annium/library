using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Extensions.Shell;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using xrest.Tools;

namespace xrest.Commands
{
    internal class GenerateCommand : AsyncCommand<GenerateCommandConfiguration>
    {
        public override string Id { get; } = "gen";
        public override string Description { get; } = "generate client";
        private readonly Generator generator;
        private readonly IShell shell;
        private readonly ILogger<GenerateCommand> logger;

        public GenerateCommand(
            Generator generator,
            IShell shell,
            ILogger<GenerateCommand> logger
        )
        {
            this.generator = generator;
            this.shell = shell;
            this.logger = logger;
        }

        public override async Task HandleAsync(GenerateCommandConfiguration cfg, CancellationToken token)
        {
            if (!Directory.Exists(cfg.Project))
                throw new DirectoryNotFoundException($"Project directory {cfg.Project} not found");

            var projectFileName = Path.Combine(cfg.Project, $"{cfg.ProjectName}.csproj");
            if (Directory.GetFiles(cfg.Project).All(x => x != projectFileName))
                throw new InvalidOperationException($"Project file {projectFileName} not found in {cfg.Project}");

            logger.Info($"Generate client for project '{cfg.ProjectName}'");

            logger.Debug("Build project");
            // await Build(cfg, token);

            logger.Debug("Load types");
            var controllers = ResolveControllerTypes(cfg);

            logger.Debug("Generate");
            generator.Generate(controllers, cfg.Output);
        }

        private Task Build(GenerateCommandConfiguration cfg, CancellationToken token) => shell
            .Cmd(
                "dotnet build",
                $"-c {cfg.Configuration}",
                $"-f {cfg.Framework}",
                Path.GetFullPath(cfg.Project)
            )
            .RunAsync(token);

        private IReadOnlyCollection<Type> ResolveControllerTypes(GenerateCommandConfiguration cfg)
        {
            var assemblyRoot = Path.Combine(cfg.Project, "bin", cfg.Configuration, cfg.Framework);
            var assemblyPath = Path.Combine(assemblyRoot, $"{cfg.ProjectName}.dll");

            var originalDirectory = Directory.GetCurrentDirectory();
            try
            {
                var assembly = new PluginLoadContext(assemblyPath).LoadFromAssemblyPath(assemblyPath);

                var result = assembly.GetExportedTypes()
                    .Where(x => (typeof(ControllerBase).IsAssignableFrom(x)))
                    .ToArray();

                return result;
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDirectory);
            }
        }

        private class PluginLoadContext : AssemblyLoadContext
        {
            private readonly AssemblyDependencyResolver resolver;

            public PluginLoadContext(string pluginPath)
            {
                resolver = new AssemblyDependencyResolver(pluginPath);
            }

            protected override Assembly? Load(AssemblyName assemblyName)
            {
                var assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);

                return assemblyPath is null ? null : LoadFromAssemblyPath(assemblyPath);
            }
        }
    }

    internal class GenerateCommandConfiguration
    {
        [Option("p", true)]
        [Help("Path to project directory.")]
        public string Project
        {
            get => project;
            set
            {
                project = Path.GetFullPath(value);
                ProjectName = Path.GetFileName(value);
            }
        }

        public string ProjectName { get; private set; } = string.Empty;

        [Option("c")]
        [Help("Build configuration. Default is Debug.")]
        public string Configuration { get; set; } = "Debug";

        [Option("f", true)]
        [Help("Framework version.")]
        public string Framework { get; set; } = string.Empty;

        [Option("o", true)]
        [Help("Output directory. Will be removed if exists.")]
        public string Output
        {
            get => output;
            set => output = Path.GetFullPath(value);
        }

        private string project = string.Empty;
        private string output = string.Empty;
    }
}