using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium.Configuration.Abstractions;
using Xc.Setup;
using static Xc.Helper;

namespace Xc.Tasks;

internal class LoadSetupTask : IFuncTask<RootSetup, string>
{
    private readonly Func<IConfigurationBuilder> _createConfigurationBuilder;

    public LoadSetupTask(Func<IConfigurationBuilder> createConfigurationBuilder)
    {
        _createConfigurationBuilder = createConfigurationBuilder;
    }

    public RootSetup Execute(string root)
    {
        var cfg = _createConfigurationBuilder().AddYamlFile(ConfigPath(root)).Build<Setup.Raw.RootSetup>();
        var source = Path.GetFullPath(Path.Combine(root, cfg.Source));

        var targets = cfg.Includes.ToDictionary(x => x.Key, x => Include(root, source, x.Value));

        foreach (var (name, target) in cfg.Targets)
        {
            if (targets.ContainsKey(name))
                throw new InvalidOperationException($"Target {name} is already registered");

            targets[name] = target.ToDictionary(x => x.Key, x => Convert(root, source, root, x.Value));
        }

        return new RootSetup(source, targets);
    }

    private IReadOnlyDictionary<string, TargetSetup> Include(string root, string source, string path)
    {
        return _createConfigurationBuilder()
            .AddYamlFile(ConfigPath(Path.Combine(root, path)))
            .Build<Dictionary<string, Setup.Raw.TargetSetup>>()
            .ToDictionary(x => x.Key, x => Convert(root, source, path, x.Value));
    }

    private TargetSetup Convert(string root, string source, string path, Setup.Raw.TargetSetup cfg)
    {
        return new TargetSetup(
            cfg.Copy.Select(x => Path.GetFullPath(FilePath(source, x))).ToArray(),
            cfg.To.Select(x =>
                    Path.GetFullPath(x.StartsWith('/') ? Path.Combine(root, x[1..]) : Path.Combine(root, path, x))
                )
                .ToArray()
        );
    }
}
