using Annium.Extensions.Arguments;

namespace Xdb.Commands;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "xdb";

    public static string Description => "Db manager";

    public Group()
    {
        Add<Migrate.Group>();
    }
}