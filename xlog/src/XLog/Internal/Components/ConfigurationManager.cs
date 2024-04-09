using System;
using System.Collections.Generic;
using System.IO;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using XLog.Components;
using Constants = Annium.Serialization.Yaml.Constants;

namespace XLog.Internal.Components;

internal class ConfigurationManager : IConfigurationManager
{
    private static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".xlog");
    private readonly ISerializer<string> _serializer;
    private readonly Configuration _config;

    public ConfigurationManager(IServiceProvider sp)
    {
        _serializer = sp.ResolveKeyed<ISerializer<string>>(SerializerKey.CreateDefault(Constants.MediaType));
        _config = File.Exists(ConfigPath) ? _serializer.Deserialize<Configuration>(File.ReadAllText(ConfigPath)) : new();
    }

    public void GraylogLogin(string name, string server, string login, string pass)
    {
        _config.Graylog[name] = new GraylogCredentials { Server = server, Login = login, Pass = pass };
        Save();
    }

    public (string server, string login, string pass)? GraylogGetCredentials(string name)
    {
        return _config.Graylog.TryGetValue(name, out var c) ? (c.Server, c.Login, c.Pass) : null;
    }

    public void GraylogLogout(string name)
    {
        _config.Graylog.Remove(name);
        Save();
    }

    private void Save()
    {
        var raw = _serializer.Serialize(_config);
        File.WriteAllText(ConfigPath, raw);
    }

    private class Configuration
    {
        public Dictionary<string, GraylogCredentials> Graylog { get; init; } = new();
    }

    private class GraylogCredentials
    {
        public string Server { get; init; } = string.Empty;
        public string Login { get; init; } = string.Empty;
        public string Pass { get; init; } = string.Empty;
    }
}
