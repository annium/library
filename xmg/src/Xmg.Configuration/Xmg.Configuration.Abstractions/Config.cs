namespace Xmg.Configuration.Abstractions;

public class Config
{
    public string Assembly { get; }

    public Config(string assembly)
    {
        Assembly = assembly;
    }
}
