using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Entrypoint;
using LibSassHost;

namespace XSass
{
    internal static class Program
    {
        private static readonly ImmutableHashSet<string> ExcludedDirectories = ImmutableHashSet.Create(
            "bin",
            "obj",
            "logs",
            "node_modules"
        );

        private static async Task CompileDirectoriesAsync(string directory)
        {
            var sassFiles = Directory.EnumerateFiles(directory)
                .Where(file => file.EndsWith(".scss", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".sass", StringComparison.OrdinalIgnoreCase));

            await CompileFilesAsync(sassFiles);

            var subDirectories = Directory.EnumerateDirectories(directory);
            foreach (var subDirectory in subDirectories)
            {
                if (ExcludedDirectories.Any(dir => subDirectory.EndsWith(dir, StringComparison.OrdinalIgnoreCase)))
                    continue;

                await CompileDirectoriesAsync(subDirectory);
            }
        }

        private static async Task CompileFilesAsync(IEnumerable<string> sassFiles)
        {
            foreach (var file in sassFiles)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Name.StartsWith("_"))
                    continue;

                var result = SassCompiler.CompileFile(file, options: new CompilationOptions { OutputStyle = OutputStyle.Compressed });

                var newFile = fileInfo.FullName.Replace(fileInfo.Extension, ".css");

                if (File.Exists(newFile) && result.CompiledContent == await File.ReadAllTextAsync(newFile))
                    continue;

                await File.WriteAllTextAsync(newFile, result.CompiledContent);
            }
        }

        private static async Task Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var searchDirectory = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
            Console.WriteLine($"Sass compile directory: {searchDirectory}");

            await Task.CompletedTask;
            // await CompileDirectoriesAsync(searchDirectory);

            // Console.WriteLine("Sass files compiled");
        }

        internal static Task<int> Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
