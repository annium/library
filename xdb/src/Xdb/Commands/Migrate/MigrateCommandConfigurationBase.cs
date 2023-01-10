using System;
using System.IO;
using Annium.Extensions.Arguments;

namespace Xdb.Commands.Migrate;

internal abstract record MigrateCommandConfigurationBase
{
    [Position(1)]
    [Help("yaml config path")]
    public string Config
    {
        get => _config;
        set
        {
            var path = Path.GetFullPath(value);
            if (!File.Exists(path))
                throw new ArgumentException($"Configuration file {path} is missing");

            _config = path;
        }
    }

    [Option("d")]
    [Help("path to directory with migrations")]
    public string Directory
    {
        get => _directory;
        set
        {
            var path = Path.GetFullPath(value);
            if (!System.IO.Directory.Exists(path))
                throw new ArgumentException($"Scripts directory {path} is missing");

            _directory = path;
        }
    }

    [Option("a")]
    [Help("path to assembly with migrations")]
    public string Assembly
    {
        get => _assembly;
        set
        {
            var path = Path.GetFullPath(value);
            if (!File.Exists(path))
                throw new ArgumentException($"Assembly {path} is missing");

            _assembly = path;
        }
    }

    private string _config = string.Empty;
    private string _directory = string.Empty;
    private string _assembly = string.Empty;
}