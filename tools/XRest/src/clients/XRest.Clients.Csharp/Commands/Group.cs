namespace XRest.Clients.Csharp.Commands;

public class Group : Annium.Extensions.Arguments.Group
{
    public override string Id => "dotnet";

    public override string Description => ".NET commands";

    public Group()
    {
        Add<GenerateCommand>();
    }
}