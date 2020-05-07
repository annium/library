namespace Xmg.Core.Views
{
    public interface IMigration
    {
        string Name { get; }
        string Version { get; }
    }
}