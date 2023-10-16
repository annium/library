using Annium.Extensions.Arguments;

namespace Xf.Commands;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "xc";

    public static string Description => "Configuration manager";

    public Group()
    {
        Add<ListCommand>();
    }
}