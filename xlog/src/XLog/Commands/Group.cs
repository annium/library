namespace XLog.Commands;

public class Group : Annium.Extensions.Arguments.Group
{
    public override string Id { get; } = string.Empty;
    public override string Description { get; } = "commands";

    public Group()
    {
        Add<ListenCommand>();
    }
}