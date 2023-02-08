namespace XRest.Commands;

internal class Group : Annium.Extensions.Arguments.Group
{
    public override string Id => "xrest";

    public override string Description => "REST client generator";

    public Group()
    {
        Add<Clients.Csharp.Commands.Group>();
        Add<Clients.TypeScript.Commands.Group>();
        Add<ParseCommand>();
    }
}