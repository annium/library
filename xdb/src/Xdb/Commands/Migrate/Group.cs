using Annium.Extensions.Arguments;

namespace Xdb.Commands.Migrate;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "migrate";

    public static string Description => "Migration commands";

    public Group()
    {
        Add<MigratePostgresqlCommand>();
    }
}