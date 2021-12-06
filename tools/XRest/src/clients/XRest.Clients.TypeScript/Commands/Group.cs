namespace XRest.Clients.TypeScript.Commands;

public class Group : Annium.Extensions.Arguments.Group
{
    public override string Id { get; } = "ts";

    public override string Description { get; } = "TypeScript commands";

    public Group()
    {
        Add<GenerateCommand>();
    }
}