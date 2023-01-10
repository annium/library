namespace Xdb.Commands.Migrate;

internal class Group : Annium.Extensions.Arguments.Group
{
    public override string Id { get; } = "migrate";

    public override string Description { get; } = "Migration commands";

    public Group()
    {
        Add<MigratePostgresqlCommand>();
    }
}