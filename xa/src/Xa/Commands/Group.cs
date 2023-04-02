using Annium.Extensions.Arguments;

namespace Xa.Commands;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "xa";

    public static string Description => "analytics";

    public Group()
    {
        Add<GlobCommand>();
    }
}