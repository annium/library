namespace Xdb.Commands;

internal class Group : Annium.Extensions.Arguments.Group
{
    public override string Id { get; } = "xdb";

    public override string Description { get; } = "Db manager";

    public Group()
    {
        Add<Migrate.Group>();
    }
}