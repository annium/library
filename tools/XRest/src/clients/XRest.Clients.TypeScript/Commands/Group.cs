namespace XRest.Clients.TypeScript.Commands;

public class Group : Annium.Extensions.Arguments.Group
{
    public override string Id => "ts";

    public override string Description => "TypeScript commands";

    public Group()
    {
        Add<GenerateCommand>();
    }
}