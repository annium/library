namespace xdomains.Commands;

internal class Group : Annium.Extensions.Arguments.Group
{
    public override string Id { get; } = "xdomains";

    public override string Description { get; } = "domains toolkit";

    public Group()
    {
        Add<CleanupCommand>();
        Add<QueryCommand>();
    }
}