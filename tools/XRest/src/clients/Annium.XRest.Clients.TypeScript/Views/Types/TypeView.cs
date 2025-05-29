namespace Annium.XRest.Clients.TypeScript.Views.Types;

internal abstract record TypeView(string Name)
{
    public abstract TypeViewEnum Type { get; }

    public override int GetHashCode() => ToString()!.GetHashCode();
}
