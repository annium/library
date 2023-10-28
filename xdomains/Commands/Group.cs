using Annium.Extensions.Arguments;

namespace xdomains.Commands;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "xdomains";

    public static string Description => "domains toolkit";

    public Group()
    {
        Add<CleanupCommand>();
        Add<QueryCommand>();
    }
}
