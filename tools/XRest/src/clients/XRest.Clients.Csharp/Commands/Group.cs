namespace XRest.Clients.Csharp.Commands;

public class Group : Annium.Extensions.Arguments.Group
{
    public override string Id => "cs";

    public override string Description => "C# commands";

    public Group()
    {
        Add<GenerateCommand>();
    }
}