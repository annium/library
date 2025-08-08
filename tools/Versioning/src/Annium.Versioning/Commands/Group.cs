using Annium.Extensions.Arguments;

namespace Annium.Versioning.Commands;

internal class Group : Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "versioning";

    public static string Description => "versioning tool";

    public Group()
    {
        Add<GetVersionCommand>();
        Add<SetVersionCommand>();
    }
}
