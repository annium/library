namespace Annium.XRest.Clients.TypeScript.Views.Types;

internal record GenericParameterView : TypeView
{
    public override TypeViewEnum Type => TypeViewEnum.GenericParameter;

    public GenericParameterView(string name)
        : base(name) { }

    public override string ToString() => Name;
}
