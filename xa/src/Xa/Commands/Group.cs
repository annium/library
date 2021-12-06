namespace Xa.Commands;

internal class Group : Annium.Extensions.Arguments.Group
{
    public override string Id { get; } = "xa";

    public override string Description { get; } = "analytics";

    public Group()
    {
        Add<GlobCommand>();
    }
}